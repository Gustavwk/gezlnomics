import { FormEvent, useEffect, useMemo, useState } from 'react';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string;

type AuthUser = { id: string; email: string };
type UserSettings = { paydayDayOfMonth: number; currencyCode: string; timezone: string };
type LedgerSummary = {
  periodStart: string;
  periodEnd: string;
  startingBalance: number;
  currentBalance: number;
  forecastBalance: number;
  daysUntilNextPayday: number;
  moneyPerDay: number;
  cumulativeSpending: number;
  currencyCode: string;
};
type IncomePeriod = {
  id: string;
  periodStartDate: string;
  periodEndDate: string;
  startingBalance: number;
  createdAt: string;
};
type Transaction = {
  id: string;
  date: string;
  amount: number;
  category: string;
  note?: string;
  kind: number;
  status: number;
};
type RecurringRule = {
  id: string;
  title: string;
  amount: number;
  category: string;
  note?: string;
  ruleKind: number;
  frequency: number;
  startDate: string;
  endDate?: string;
  isActive: boolean;
};

const kindLabels: Record<number, string> = {
  0: 'Expense Actual',
  1: 'Expense Planned',
  2: 'Savings Out',
  3: 'Savings In'
};

const frequencyLabels: Record<number, string> = {
  0: 'Weekly',
  1: 'Monthly',
  2: 'Yearly'
};

async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {})
    }
  });

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function App() {
  const today = new Date().toISOString().slice(0, 10);

  const [authMode, setAuthMode] = useState<'login' | 'signup'>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [authUser, setAuthUser] = useState<AuthUser | null>(null);
  const [authError, setAuthError] = useState('');
  const [loading, setLoading] = useState(true);

  const [settings, setSettings] = useState<UserSettings | null>(null);
  const [summary, setSummary] = useState<LedgerSummary | null>(null);
  const [periods, setPeriods] = useState<IncomePeriod[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [recurringRules, setRecurringRules] = useState<RecurringRule[]>([]);

  const [filterKind, setFilterKind] = useState<string>('');

  const [periodStartDate, setPeriodStartDate] = useState(today);
  const [periodEndDate, setPeriodEndDate] = useState(today);
  const [periodStartingBalance, setPeriodStartingBalance] = useState('0');

  const [txDate, setTxDate] = useState(today);
  const [txAmount, setTxAmount] = useState('0');
  const [txCategory, setTxCategory] = useState('General');
  const [txNote, setTxNote] = useState('');
  const [txKind, setTxKind] = useState('0');

  const [ruleTitle, setRuleTitle] = useState('');
  const [ruleAmount, setRuleAmount] = useState('0');
  const [ruleCategory, setRuleCategory] = useState('General');
  const [ruleNote, setRuleNote] = useState('');
  const [ruleKind, setRuleKind] = useState('1');
  const [ruleFrequency, setRuleFrequency] = useState('1');
  const [ruleStartDate, setRuleStartDate] = useState(today);
  const [ruleEndDate, setRuleEndDate] = useState('');

  const filteredTransactions = useMemo(() => {
    if (!filterKind) {
      return transactions;
    }

    const kind = Number(filterKind);
    return transactions.filter((x) => x.kind === kind);
  }, [transactions, filterKind]);

  async function loadAll() {
    const [settingsData, summaryData, periodsData, transactionsData, recurringData] = await Promise.all([
      api<UserSettings>('/api/settings/'),
      api<LedgerSummary>('/api/ledger/summary'),
      api<IncomePeriod[]>('/api/income-periods/'),
      api<Transaction[]>('/api/transactions/'),
      api<RecurringRule[]>('/api/recurring-rules/')
    ]);

    setSettings(settingsData);
    setSummary(summaryData);
    setPeriods(periodsData);
    setTransactions(transactionsData);
    setRecurringRules(recurringData);
  }

  async function refresh() {
    try {
      await loadAll();
    } catch {
      setAuthUser(null);
      setSettings(null);
      setSummary(null);
      setPeriods([]);
      setTransactions([]);
      setRecurringRules([]);
    }
  }

  useEffect(() => {
    (async () => {
      try {
        const me = await api<AuthUser>('/api/auth/me');
        setAuthUser(me);
        await loadAll();
      } catch {
        setAuthUser(null);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  async function handleAuthSubmit(event: FormEvent) {
    event.preventDefault();
    setAuthError('');

    try {
      const path = authMode === 'signup' ? '/api/auth/signup' : '/api/auth/login';
      const user = await api<AuthUser>(path, {
        method: 'POST',
        body: JSON.stringify({ email, password })
      });

      setAuthUser(user);
      await loadAll();
      setPassword('');
    } catch {
      setAuthError('Login/signup fejlede. Tjek email/password.');
    }
  }

  async function logout() {
    await api<void>('/api/auth/logout', { method: 'POST' });
    setAuthUser(null);
  }

  async function saveSettings(event: FormEvent) {
    event.preventDefault();
    if (!settings) return;

    await api<UserSettings>('/api/settings/', {
      method: 'PUT',
      body: JSON.stringify(settings)
    });
    await refresh();
  }

  async function createPeriod(event: FormEvent) {
    event.preventDefault();

    await api<IncomePeriod>('/api/income-periods/', {
      method: 'POST',
      body: JSON.stringify({
        periodStartDate,
        periodEndDate,
        startingBalance: Number(periodStartingBalance)
      })
    });

    await refresh();
  }

  async function removePeriod(id: string) {
    await api<void>(`/api/income-periods/${id}`, { method: 'DELETE' });
    await refresh();
  }

  async function createTransaction(event: FormEvent) {
    event.preventDefault();

    await api<Transaction>('/api/transactions/', {
      method: 'POST',
      body: JSON.stringify({
        date: txDate,
        amount: Number(txAmount),
        category: txCategory,
        note: txNote || null,
        kind: Number(txKind),
        status: 0
      })
    });

    await refresh();
  }

  async function removeTransaction(id: string) {
    await api<void>(`/api/transactions/${id}`, { method: 'DELETE' });
    await refresh();
  }

  async function createRecurringRule(event: FormEvent) {
    event.preventDefault();

    await api<RecurringRule>('/api/recurring-rules/', {
      method: 'POST',
      body: JSON.stringify({
        title: ruleTitle,
        amount: Number(ruleAmount),
        category: ruleCategory,
        note: ruleNote || null,
        ruleKind: Number(ruleKind),
        frequency: Number(ruleFrequency),
        startDate: ruleStartDate,
        endDate: ruleEndDate || null,
        isActive: true
      })
    });

    await refresh();
  }

  async function removeRecurringRule(id: string) {
    await api<void>(`/api/recurring-rules/${id}`, { method: 'DELETE' });
    await refresh();
  }

  if (loading) {
    return <main className="app"><p>Loading...</p></main>;
  }

  if (!authUser) {
    return (
      <main className="app">
        <header className="panel">
          <h1>Gezlnomics</h1>
          <p>Manual true ledger: startsaldo, udgifter og penge/dag.</p>
        </header>

        <section className="panel">
          <div className="tabs">
            <button onClick={() => setAuthMode('login')} className={authMode === 'login' ? 'active' : ''}>Login</button>
            <button onClick={() => setAuthMode('signup')} className={authMode === 'signup' ? 'active' : ''}>Sign up</button>
          </div>

          <form onSubmit={handleAuthSubmit} className="grid">
            <label>
              Email
              <input value={email} onChange={(e) => setEmail(e.target.value)} type="email" required />
            </label>
            <label>
              Password
              <input value={password} onChange={(e) => setPassword(e.target.value)} type="password" required minLength={8} />
            </label>
            <button type="submit">{authMode === 'login' ? 'Login' : 'Create account'}</button>
          </form>
          {authError && <p className="error">{authError}</p>}
        </section>
      </main>
    );
  }

  return (
    <main className="app">
      <header className="panel row between">
        <div>
          <h1>Gezlnomics</h1>
          <p>Signed in as {authUser.email}</p>
        </div>
        <div className="row">
          <button onClick={refresh}>Refresh</button>
          <button onClick={logout}>Logout</button>
        </div>
      </header>

      {summary && (
        <section className="panel kpis">
          <article>
            <h3>Current</h3>
            <strong>{summary.currentBalance.toFixed(2)} {summary.currencyCode}</strong>
          </article>
          <article>
            <h3>Forecast</h3>
            <strong>{summary.forecastBalance.toFixed(2)} {summary.currencyCode}</strong>
          </article>
          <article>
            <h3>Penge/dag</h3>
            <strong>{summary.moneyPerDay.toFixed(2)} {summary.currencyCode}</strong>
          </article>
          <article>
            <h3>Dage til løn</h3>
            <strong>{summary.daysUntilNextPayday}</strong>
          </article>
        </section>
      )}

      {settings && (
        <section className="panel">
          <h2>Settings</h2>
          <form onSubmit={saveSettings} className="grid three">
            <label>
              Payday day
              <input
                type="number"
                min={1}
                max={31}
                value={settings.paydayDayOfMonth}
                onChange={(e) => setSettings({ ...settings, paydayDayOfMonth: Number(e.target.value) })}
              />
            </label>
            <label>
              Currency
              <input value={settings.currencyCode} onChange={(e) => setSettings({ ...settings, currencyCode: e.target.value })} />
            </label>
            <label>
              Timezone
              <input value={settings.timezone} onChange={(e) => setSettings({ ...settings, timezone: e.target.value })} />
            </label>
            <button type="submit">Save settings</button>
          </form>
        </section>
      )}

      <section className="panel">
        <h2>Income Periods (startsaldo)</h2>
        <form onSubmit={createPeriod} className="grid three">
          <label>
            Start
            <input type="date" value={periodStartDate} onChange={(e) => setPeriodStartDate(e.target.value)} required />
          </label>
          <label>
            End
            <input type="date" value={periodEndDate} onChange={(e) => setPeriodEndDate(e.target.value)} required />
          </label>
          <label>
            Startsaldo
            <input type="number" step="0.01" value={periodStartingBalance} onChange={(e) => setPeriodStartingBalance(e.target.value)} required />
          </label>
          <button type="submit">Add period</button>
        </form>
        <ul className="list">
          {periods.map((p) => (
            <li key={p.id} className="row between">
              <span>{p.periodStartDate} ? {p.periodEndDate}: {p.startingBalance.toFixed(2)}</span>
              <button onClick={() => removePeriod(p.id)}>Delete</button>
            </li>
          ))}
        </ul>
      </section>

      <section className="panel">
        <h2>Transactions</h2>
        <form onSubmit={createTransaction} className="grid four">
          <label>
            Date
            <input type="date" value={txDate} onChange={(e) => setTxDate(e.target.value)} required />
          </label>
          <label>
            Amount
            <input type="number" step="0.01" value={txAmount} onChange={(e) => setTxAmount(e.target.value)} required />
          </label>
          <label>
            Category
            <input value={txCategory} onChange={(e) => setTxCategory(e.target.value)} required />
          </label>
          <label>
            Kind
            <select value={txKind} onChange={(e) => setTxKind(e.target.value)}>
              {Object.entries(kindLabels).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </label>
          <label className="span-3">
            Note
            <input value={txNote} onChange={(e) => setTxNote(e.target.value)} />
          </label>
          <button type="submit">Add transaction</button>
        </form>

        <div className="row">
          <label>
            Filter kind
            <select value={filterKind} onChange={(e) => setFilterKind(e.target.value)}>
              <option value="">All</option>
              {Object.entries(kindLabels).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </label>
        </div>

        <ul className="list">
          {filteredTransactions.map((t) => (
            <li key={t.id} className="row between">
              <span>{t.date} • {kindLabels[t.kind]} • {t.category} • {t.amount.toFixed(2)}</span>
              <button onClick={() => removeTransaction(t.id)}>Delete</button>
            </li>
          ))}
        </ul>
      </section>

      <section className="panel">
        <h2>Recurring Rules</h2>
        <form onSubmit={createRecurringRule} className="grid four">
          <label>
            Title
            <input value={ruleTitle} onChange={(e) => setRuleTitle(e.target.value)} required />
          </label>
          <label>
            Amount
            <input type="number" step="0.01" value={ruleAmount} onChange={(e) => setRuleAmount(e.target.value)} required />
          </label>
          <label>
            Category
            <input value={ruleCategory} onChange={(e) => setRuleCategory(e.target.value)} required />
          </label>
          <label>
            Kind
            <select value={ruleKind} onChange={(e) => setRuleKind(e.target.value)}>
              {Object.entries(kindLabels).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </label>
          <label>
            Frequency
            <select value={ruleFrequency} onChange={(e) => setRuleFrequency(e.target.value)}>
              {Object.entries(frequencyLabels).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </label>
          <label>
            Start
            <input type="date" value={ruleStartDate} onChange={(e) => setRuleStartDate(e.target.value)} required />
          </label>
          <label>
            End
            <input type="date" value={ruleEndDate} onChange={(e) => setRuleEndDate(e.target.value)} />
          </label>
          <label>
            Note
            <input value={ruleNote} onChange={(e) => setRuleNote(e.target.value)} />
          </label>
          <button type="submit">Add rule</button>
        </form>

        <ul className="list">
          {recurringRules.map((r) => (
            <li key={r.id} className="row between">
              <span>{r.title} • {kindLabels[r.ruleKind]} • {frequencyLabels[r.frequency]} • {r.amount.toFixed(2)}</span>
              <button onClick={() => removeRecurringRule(r.id)}>Delete</button>
            </li>
          ))}
        </ul>
      </section>
    </main>
  );
}

export default App;
