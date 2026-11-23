import { useEffect, useState } from 'react';
import { api, errorMessage } from './api/client';

type Health = { status: string; database: string; time: string };

export default function App() {
  const [health, setHealth] = useState<Health | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api<Health>('/api/health')
      .then(setHealth)
      .catch((e: unknown) => setError(errorMessage(e)));
  }, []);

  return (
    <div className="wrap">
      <h1>Placement Hub</h1>
      <p className="mu">Setting things up. Let's check that the app can talk to the server.</p>

      <div className="card" style={{ marginTop: 22, maxWidth: 480 }}>
        {!health && !error && <p>Checking the server…</p>}
        {error && (
          <p>
            <span className="chip bad">Not connected</span> {error}
          </p>
        )}
        {health && (
          <>
            <p>
              <span className="chip ok">Connected</span> The server is {health.status.toLowerCase()}.
            </p>
            <p className="mu">Database: {health.database}</p>
          </>
        )}
      </div>
    </div>
  );
}