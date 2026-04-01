import pandas as pd
import requests
import json
from logic.rules import apply_audit_rules

# Configuration
API_URL = "http://localhost:5122/api/ingest/batch"
CSV_FILE = "../../test_data.csv"

def run_ingestion():
    try:
        # 1. Load Data
        print(f"--- Reading {CSV_FILE} ---")
        df = pd.read_csv(CSV_FILE)
        
        # 2. Map to .NET DTO (Match the C# Record names exactly)
        # .NET expects: EmployeeName, DepartmentName, CategoryName, Description, Amount, ProcessedAt
        payload = []
        for _, row in df.iterrows():
            payload.append({
                "EmployeeName": row['EmployeeName'],
                "DepartmentName": row['Department'],
                "CategoryName": row['Category'],
                "Description": row['Description'],
                "Amount": float(row['Amount']),
                "ProcessedAt": row['Date']
            })

        payload = apply_audit_rules(payload)
            
        # 3. Fire the Request
        print(f"--- Sending {len(payload)} records to Ledger ---")
        response = requests.post(
            API_URL, 
            json=payload, 
            headers={"Content-Type": "application/json"}
        )
        
        if response.status_code == 201:
            print("Successfully Ingested to SQLite.")
        else:
            print(f"Failed: {response.status_code} - {response.text}")

    except Exception as e:
        print(f"Error: {str(e)}")

if __name__ == "__main__":
    run_ingestion()