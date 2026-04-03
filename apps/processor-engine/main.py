import pandas as pd
import pika
import json
import os
import time
import shutil
import logging
from datetime import datetime
from logic.rules import apply_audit_rules

# --- Configuration & Logging ---
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s [%(levelname)s] %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)
logger = logging.getLogger("SentinelWorker")

RABBIT_HOST = os.getenv('RABBITMQ_HOST', 'rabbitmq')
QUEUE_NAME = os.getenv('RABBITMQ_QUEUE', 'audit_queue')
BASE_PATH = os.getenv('DATA_PATH', '/app/data')

# Domain-driven directory structure
INPUT_DIR = os.path.join(BASE_PATH, "input")
PROCESSED_DIR = os.path.join(BASE_PATH, "processed")
ERROR_DIR = os.path.join(BASE_PATH, "errors")

class RabbitClient:
    """Handles persistent connection and publishing logic with self-healing."""
    def __init__(self):
        self.params = pika.ConnectionParameters(
            host=RABBIT_HOST, 
            heartbeat=600,
            blocked_connection_timeout=300
        )
        self.connection = None
        self.channel = None
        self._connect()

    def _connect(self):
        for attempt in range(1, 11):
            try:
                if self.connection and not self.connection.is_closed:
                    self.connection.close()
                self.connection = pika.BlockingConnection(self.params)
                self.channel = self.connection.channel()
                self.channel.queue_declare(queue=QUEUE_NAME, durable=True)
                self.channel.confirm_delivery() # PRO: Ensure RabbitMQ ACKs receipt
                logger.info(f"Successfully connected to RabbitMQ.")
                return
            except Exception as e:
                logger.warning(f"Connection failed ({attempt}/10): {e}. Retrying in 5s...")
                time.sleep(5)
        raise SystemExit("Critical Error: RabbitMQ unreachable.")

    def publish(self, payload):
        try:
            if not self.connection or self.connection.is_closed:
                self._connect()
            
            self.channel.basic_publish(
                exchange='',
                routing_key=QUEUE_NAME,
                body=json.dumps(payload),
                properties=pika.BasicProperties(
                    delivery_mode=2,
                    content_type='application/json',
                    timestamp=int(time.time())
                )
            )
            return True
        except Exception as e:
            logger.error(f"Publishing error: {e}")
            return False

def process_files(rabbit: RabbitClient):
    """Orchestrates file ingestion using vectorized operations."""
    for d in [INPUT_DIR, PROCESSED_DIR, ERROR_DIR]:
        os.makedirs(d, exist_ok=True)

    files = [f for f in os.listdir(INPUT_DIR) if f.lower().endswith('.csv')]
    
    for filename in files:
        source_path = os.path.join(INPUT_DIR, filename)
        logger.info(f"Ingesting: {filename}")
        
        try:
            # PRO: Vectorized cleaning is faster and handles 'NaN' better than loops
            df = pd.read_csv(source_path)
            
            # Ensure required columns exist before processing
            required_cols = {'EmployeeName', 'Department', 'Category', 'Description', 'Amount', 'Date'}
            if not required_cols.issubset(df.columns):
                missing = required_cols - set(df.columns)
                raise ValueError(f"Missing columns: {missing}")

            # Vectorized renaming and type conversion
            df = df.rename(columns={
                'Department': 'DepartmentName',
                'Category': 'CategoryName',
                'Date': 'ProcessedAt'
            })
            df['Amount'] = pd.to_numeric(df['Amount'], errors='coerce').fillna(0.0)
            
            # Convert to List of Dicts
            raw_data = df.to_dict(orient='records')

            # Business Rules Layer
            processed_payload = apply_audit_rules(raw_data)
            
            # Atomic operation: Publish THEN Move
            if rabbit.publish(processed_payload):
                ts = datetime.now().strftime("%Y%m%d_%H%M%S")
                shutil.move(source_path, os.path.join(PROCESSED_DIR, f"{ts}_{filename}"))
                logger.info(f"Success: {filename} moved to storage.")
            else:
                logger.error(f"Postponed: Broker NACK for {filename}.")

        except Exception as e:
            logger.error(f"Failed {filename}: {str(e)}")
            ts = datetime.now().strftime("%Y%m%d_%H%M%S")
            shutil.move(source_path, os.path.join(ERROR_DIR, f"err_{ts}_{filename}"))

if __name__ == "__main__":
    logger.info("Sentinel Engine monitoring landing zone...")
    client = RabbitClient()
    
    try:
        while True:
            process_files(client)
            time.sleep(10)
    except KeyboardInterrupt:
        logger.info("Graceful shutdown initiated.")
        if client.connection:
            client.connection.close()