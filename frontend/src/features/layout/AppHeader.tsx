import type { AuthUser } from '../../types/models';

type Props = {
  bruger: AuthUser;
  onLogout: () => Promise<void>;
};

export function AppHeader({ bruger, onLogout }: Props) {
  return (
    <header className="panel row mellem">
      <div>
        <h1>Gezlnomics</h1>
        <p>Logget ind som {bruger.username}</p>
      </div>
      <div className="row">
        <button onClick={onLogout}>Log ud</button>
      </div>
    </header>
  );
}
