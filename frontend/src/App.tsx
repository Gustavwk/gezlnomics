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
    periodeValg,
    valgtPeriodeNoegle,
    setStartSaldo,
    handleAuth,
    handleLogout,
    gemStartsaldo,
    opretTransaktion,
    sletTransaktion,
    opretFastUdgift,
    sletFastUdgift,
    vaelgPeriode,
    opretNaestePeriode
  } = useLedgerApp();

  if (indlaeser) {
    return (
      <main className="app">
        <p className="panel">Indlæser...</p>
      </main>
    );
  }

  if (!bruger) {
    return (
      <main className="app">
        <section className="auth-center">
          <AuthPanel fejl={fejl} onSubmit={handleAuth} />
        </section>
      </main>
    );
  }

  return (
    <main className="app">
      <section className="app-grid">
        <div className="area-header">
          <AppHeader bruger={bruger} onLogout={handleLogout} />
        </div>

        {summary && (
          <div className="area-ledger">
            <LedgerPanel
              summary={summary}
              startSaldo={startSaldo}
              periodeValg={periodeValg}
              valgtPeriodeNoegle={valgtPeriodeNoegle}
              setStartSaldo={setStartSaldo}
              onSaveStartsaldo={gemStartsaldo}
              onVaelgPeriode={vaelgPeriode}
              onOpretNaestePeriode={opretNaestePeriode}
            />
          </div>
        )}

        <div className="area-transactions">
          <TransactionsPanel
            transaktioner={transaktioner}
            onCreate={opretTransaktion}
            onDelete={sletTransaktion}
          />
        </div>

        <div className="area-recurring">
          <FasteUdgifterPanel
            fasteUdgifter={fasteUdgifter}
            onCreate={opretFastUdgift}
            onDelete={sletFastUdgift}
          />
        </div>
      </section>

      {fejl && <p className="fejl">{fejl}</p>}
    </main>
  );
}

export default App;
