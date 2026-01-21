import { useState } from 'react';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string;

function App() {
  const [pingResponse, setPingResponse] = useState<string>('');
  const [loading, setLoading] = useState(false);

  const handlePing = async () => {
    setLoading(true);
    try {
      const response = await fetch(`${apiBaseUrl}/api/ping`);
      const text = await response.text();
      setPingResponse(text);
    } catch (error) {
      setPingResponse('Unable to reach backend.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app">
      <header>
        <h1>Gezlnomics</h1>
        <p>Bootstrap UI for the future budgeting MVP.</p>
      </header>
      <section className="card">
        <button type="button" onClick={handlePing} disabled={loading}>
          {loading ? 'Pinging…' : 'Ping backend'}
        </button>
        {pingResponse && <p className="response">{pingResponse}</p>}
      </section>
    </div>
  );
}

export default App;
