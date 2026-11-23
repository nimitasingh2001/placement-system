import { useState, type FormEvent } from 'react';
import { Link, Navigate } from 'react-router';
import { errorMessage } from '../api/client';
import { useAuth } from '../auth/context';

export default function LoginPage() {
  const { session, login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (session) return <Navigate to="/" replace />;   // already signed in

  async function onSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await login(email.trim(), password);   // on success the page above redirects by itself
    } catch (e) {
      setError(errorMessage(e));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 24 }}>
      <form className="card stack" style={{ width: '100%', maxWidth: 420, padding: 32 }} onSubmit={onSubmit}>
        <div>
          <h1>Welcome back</h1>
          <p className="mu">Log in to see jobs that fit your skills.</p>
        </div>
        <div>
          <label htmlFor="email">Email</label>
          <input id="email" type="email" autoComplete="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div>
          <label htmlFor="password">Password</label>
          <input id="password" type="password" autoComplete="current-password" required value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        {error && <p role="alert" style={{ color: '#c92a2a' }}>{error}</p>}
        <button className="btn p" type="submit" disabled={busy}>{busy ? 'Logging in…' : 'Log in'}</button>
        <p className="mu">First time here? Create your account with your roll number and the email your college has on file.</p>
        <Link className="btn" to="/register" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', textDecoration: 'none' }}>
          Create my account
        </Link>
      </form>
    </div>
  );
}