import pandas as pd
from typing import List, Dict

def parse_financial_csv(file_content: bytes) -> List[Dict]:
    # Load the buffer into Pandas
    df = pd.read_csv(pd.io.common.BytesIO(file_content))
    
    # Normalize headers (Force lowercase/no spaces)
    df.columns = [c.lower().replace(' ', '_') for c in df.columns]
    
    # Logic: Convert DF to a list of dicts that match the C# 'Cost' record
    costs = []
    for _, row in df.iterrows():
        costs.append({
            "employee_name": row['employeename'],
            "department_name": row['department'],
            "category_name": row['category'],
            "description": row['description'],
            "amount": float(row['amount']),
            "processed_at": row['date']
        })
    
    return costs