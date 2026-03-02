import { FormEvent, useState } from 'react';
import type { AuthMode } from '../../types/models';

type Props = {
  fejl: string;
  onSubmit: (mode: AuthMode, username: string, password: string) => Promise<void>;
};

export function AuthPanel({ fejl, onSubmit }: Props) {
  const [authMode, setAuthMode] = useState<AuthMode>('login');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onSubmit(authMode, username, password);
    setPassword('');
  }

  return (
    <section className="panel auth-panel">
      <h1>Gezlnomics</h1>
      <p>Log ind for at fortsætte.</p>

      <div className="row tabs">
        <button className={authMode === 'login' ? 'active' : ''} onClick={() => setAuthMode('login')}>
          Log ind
        </button>
        <button className={authMode === 'signup' ? 'active' : ''} onClick={() => setAuthMode('signup')}>
          Opret bruger
        </button>
      </div>

      <form onSubmit={handleSubmit} className="grid auth-form">
        <label>
          Brugernavn
          <input
            type="text"
            minLength={3}
            maxLength={32}
            pattern="[A-Za-z0-9._-]+"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </label>
        <label>
          Adgangskode
          <input type="password" minLength={8} value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>
        <button type="submit">{authMode === 'login' ? 'Log ind' : 'Opret'}</button>
      </form>

      {fejl && <p className="fejl">{fejl}</p>}
    </section>
  );
}
