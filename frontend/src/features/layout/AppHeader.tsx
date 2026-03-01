import type { AuthUser } from '../../types/models';

type Props = {
  bruger: AuthUser;
  onRefresh: () => void;
  onLogout: () => Promise<void>;
};

export function AppHeader({ bruger, onRefresh, onLogout }: Props) {
  return (
    <header className="panel row mellem">
      <div>
        <h1>Gezlnomics</h1>
        <p>Logget ind som {bruger.email}</p>
      </div>
      <div className="row">
        <button onClick={onRefresh}>Opdater</button>
        <button onClick={onLogout}>Log ud</button>
      </div>
    </header>
  );
}
