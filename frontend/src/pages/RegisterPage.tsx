import { useState, type FormEvent } from 'react';
import { Link, Navigate } from 'react-router';
import { errorMessage } from '../api/client';
import { useAuth } from '../auth/context';

export default function RegisterPage() {
  const { session, register } = useAuth();
  const [rollNo, setRollNo] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [again, setAgain] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (session) return <Navigate to="/" replace />;

  async function onSubmit(event: FormEvent) {
    event.preventDefault();
    if (password !== again) {
      setError("The two passwords don't match.");
      return;
    }
    setError(null);
    setBusy(true);
    try {
      await register(rollNo.trim(), email.trim(), password);
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
          <h1>Create your account</h1>
          <p className="mu">Use the roll number and email your college has on file for you.</p>
        </div>
        <div>
          <label htmlFor="roll">Roll number</label>
          <input id="roll" required value={rollNo} onChange={(e) => setRollNo(e.target.value)} />
        </div>
        <div>
          <label htmlFor="email">College email</label>
          <input id="email" type="email" autoComplete="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div>
          <label htmlFor="password">Choose a password (at least 8 characters)</label>
          <input id="password" type="password" autoComplete="new-password" minLength={8} required value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        <div>
          <label htmlFor="again">Type the password again</label>
          <input id="again" type="password" autoComplete="new-password" required value={again} onChange={(e) => setAgain(e.target.value)} />
        </div>
        {error && <p role="alert" style={{ color: '#c92a2a' }}>{error}</p>}
        <button className="btn p" type="submit" disabled={busy}>{busy ? 'Creating…' : 'Create my account'}</button>
        <p className="mu">Already have an account? <Link to="/login">Log in</Link></p>
      </form>
    </div>
  );
}