import { AuthPanel } from './features/auth/AuthPanel';
import { AppHeader } from './features/layout/AppHeader';
import { LedgerPanel } from './features/ledger/LedgerPanel';
import { FasteUdgifterPanel } from './features/recurring/FasteUdgifterPanel';
import { TransactionsPanel } from './features/transactions/TransactionsPanel';
import { useLedgerApp } from './hooks/useLedgerApp';

function App() {
  const {
    bruger,
    indlaeser,
    fejl,
    summary,
    transaktioner,
    fasteUdgifter,
    startSaldo,
    setStartSaldo,
    handleAuth,
    handleLogout,
    opdater,
    gemStartsaldo,
    opretTransaktion,
    sletTransaktion,
    opretFastUdgift,
    sletFastUdgift
  } = useLedgerApp();

  if (indlaeser) {
    return (
      <main className="app">
        <p>Indlæser...</p>
      </main>
    );
  }

  if (!bruger) {
    return (
      <main className="app">
        <AuthPanel fejl={fejl} onSubmit={handleAuth} />
      </main>
    );
  }

  return (
    <main className="app">
      <AppHeader bruger={bruger} onRefresh={opdater} onLogout={handleLogout} />

      {summary && (
        <LedgerPanel
          summary={summary}
          startSaldo={startSaldo}
          setStartSaldo={setStartSaldo}
          onSaveStartsaldo={gemStartsaldo}
        />
      )}

      <TransactionsPanel
        transaktioner={transaktioner}
        onCreate={opretTransaktion}
        onDelete={sletTransaktion}
      />

      <FasteUdgifterPanel
        fasteUdgifter={fasteUdgifter}
        onCreate={opretFastUdgift}
        onDelete={sletFastUdgift}
      />

      {fejl && <p className="fejl">{fejl}</p>}
    </main>
  );
}

export default App;
