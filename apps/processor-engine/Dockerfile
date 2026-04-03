# Use a slim Python image for speed
FROM python:3.11-slim

WORKDIR /app

# Install system dependencies for Pandas
RUN apt-get update && apt-get install -y --no-install-recommends \
    gcc \
    python3-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the engine code
COPY . .

# Run the ingestion script
CMD ["python", "main.py"]