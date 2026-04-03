export default async function Dashboard() {
  // Use the Internal Docker DNS name: 'ledger-api'
  // Use the Internal Port: '5000' (as defined in your .NET logs)
  const apiUrl = process.env.API_URL || 'http://ledger-api:5000';
  
  try {
    const res = await fetch(`${apiUrl}/api/dashboard/stats`, { 
      cache: 'no-store' 
    });

    if (!res.ok) {
      throw new Error(`API responded with status: ${res.status}`);
    }

    const data = await res.json();

    return (
      <main className="p-8 bg-zinc-950 text-zinc-100 min-h-screen">
        <h1 className="text-3xl font-bold mb-8">Sentinel Ledger Overview</h1>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-12">
          <div className="p-6 bg-zinc-900 border border-zinc-800 rounded-lg">
            <p className="text-zinc-400 text-sm uppercase tracking-wider">Total System Spend</p>
            <p className="text-4xl font-mono text-green-400">${data.totalSpend.toLocaleString()}</p>
          </div>
          <div className="p-6 bg-zinc-900 border border-zinc-800 rounded-lg">
            <p className="text-zinc-400 text-sm uppercase tracking-wider">Active Employees</p>
            <p className="text-4xl font-mono">{data.employeeCount}</p>
          </div>
        </div>

        <h2 className="text-xl font-semibold mb-4">Latest Transactions</h2>
        <div className="bg-zinc-900 border border-zinc-800 rounded-lg overflow-hidden">
          {data.recent.map((cost: any, i: number) => (
            <div key={i} className="p-4 border-b border-zinc-800 flex justify-between">
              <span>{cost.description}</span>
              <span className="font-mono text-zinc-400">${cost.amount}</span>
            </div>
          ))}
        </div>
      </main>
    );
  } catch (error) {
    // This prevents the whole dashboard from crashing if the API is still booting
    return (
      <div className="p-8 text-red-400 font-mono">
        Ledger API connection failed. Retrying sync...
      </div>
    );
  }
}