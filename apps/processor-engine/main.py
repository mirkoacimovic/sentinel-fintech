from fastapi import FastAPI, UploadFile, File
import pandas as pd
import io

app = FastAPI(title="Sentinel Processor Engine")

@app.get("/health")
def health():
    return {"status": "healthy"}

@app.post("/ingest")
async def ingest_csv(file: UploadFile = File(...)):
    # Read CSV into Pandas
    contents = await file.read()
    df = pd.read_csv(io.BytesIO(contents))
    
    # Simple conversion to dict for now
    data = df.to_dict(orient="records")
    
    return {
        "filename": file.filename,
        "row_count": len(df),
        "data": data # This is what we will push to .NET
    }