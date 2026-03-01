import { FormEvent, useState } from 'react';
import type { AuthMode } from '../../types/models';

type Props = {
  fejl: string;
  onSubmit: (mode: AuthMode, email: string, password: string) => Promise<void>;
};

export function AuthPanel({ fejl, onSubmit }: Props) {
  const [authMode, setAuthMode] = useState<AuthMode>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onSubmit(authMode, email, password);
    setPassword('');
  }

  return (
    <section className="panel">
      <h1>Gezlnomics</h1>
      <p>Log ind for at se din lønperiode og dine udgifter.</p>

      <div className="row tabs">
        <button className={authMode === 'login' ? 'active' : ''} onClick={() => setAuthMode('login')}>
          Log ind
        </button>
        <button className={authMode === 'signup' ? 'active' : ''} onClick={() => setAuthMode('signup')}>
          Opret bruger
        </button>
      </div>

      <form onSubmit={handleSubmit} className="grid to">
        <label>
          E-mail
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
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
