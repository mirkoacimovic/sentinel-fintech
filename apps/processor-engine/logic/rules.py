def apply_audit_rules(payload: list):
    """
    Scans the batch for anomalies before sending to .NET.
    """
    for record in payload:
        # Rule 1: High Spend Flag
        if record['Amount'] > 1000:
            record['Description'] = f"[FLAG: HIGH SPEND] {record['Description']}"
        
        # Rule 2: Department-specific logic (e.g., Engineering hardware limits)
        if record['DepartmentName'] == 'Engineering' and record['CategoryName'] == 'Hardware':
            if record['Amount'] > 250:
                 record['Description'] = f"[FLAG: CAP EXCEED] {record['Description']}"
                 
    return payload