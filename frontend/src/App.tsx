import { useMemo, useState } from 'react';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string;

type ForecastResult = {
  userId: string;
  year: number;
  month: number;
  moneyPerDay: number | null;
  moneyPerDayTomorrow: number | null;
  remainingBudget: number;
  possibleSavings: number | null;
  daysUntilGoal: number | null;
  scenarios: Array<{ label: string; moneyPerDayAfterScenario: number | null }>;
};

function App() {
  const now = useMemo(() => new Date(), []);
  const [userId, setUserId] = useState('11111111-1111-1111-1111-111111111111');
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [desiredMoneyPerDay, setDesiredMoneyPerDay] = useState('250');
  const [periodMode, setPeriodMode] = useState<'RestOfMonth' | 'ThisAndNextMonth'>('RestOfMonth');
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [result, setResult] = useState<ForecastResult | null>(null);

  const saveDemoDataAndForecast = async () => {
    setLoading(true);
    setErrorMessage('');

    try {
      const monthStart = `${year}-${String(month).padStart(2, '0')}-01`;
      const putResponse = await fetch(`${apiBaseUrl}/api/users/${userId}/months/${year}/${month}/cashflow`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          startBalance: 18000,
          savingsStart: 40000,
          withdrawnFromSavings: 1500,
          incomes: [
            { date: monthStart, amount: 22000, label: 'Salary' }
          ],
          fixedExpenses: [
            { name: 'Rent', amount: 8500, isActive: true, dueDayOfMonth: 1, frequency: 'Monthly', category: 'Housing' },
            { name: 'Insurance', amount: 900, isActive: true, dueDayOfMonth: 10, frequency: 'Monthly', category: 'Insurance' },
            { name: 'VPN', amount: 89, isActive: true, dueDayOfMonth: 14, frequency: 'Monthly', category: 'Subscriptions' }
          ],
          variableExpenses: [
            { date: monthStart, amount: 1200, label: 'Groceries planned' }
          ],
          transactions: [
            { date: monthStart, amount: 300, label: 'Coffee and lunch' },
            { date: monthStart, amount: 550, label: 'Transport card' }
          ]
        })
      });

      if (!putResponse.ok) {
        throw new Error('Failed to save user month data.');
      }

      const postResponse = await fetch(`${apiBaseUrl}/api/users/${userId}/months/${year}/${month}/forecast`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          periodMode,
          desiredMoneyPerDay: Number(desiredMoneyPerDay)
        })
      });

      if (!postResponse.ok) {
        throw new Error('Failed to calculate forecast.');
      }

      const forecast = (await postResponse.json()) as ForecastResult;
      setResult(forecast);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Unexpected error.');
      setResult(null);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app">
      <header>
        <h1>Gezlnomics</h1>
        <p>User-month cashflow forecast with daily spend guidance.</p>
      </header>

      <section className="card">
        <h2>Forecast per user</h2>
        <div className="grid">
          <label>
            User ID
            <input value={userId} onChange={(e) => setUserId(e.target.value)} />
          </label>
          <label>
            Year
            <input type="number" value={year} onChange={(e) => setYear(Number(e.target.value))} />
          </label>
          <label>
            Month
            <input type="number" min={1} max={12} value={month} onChange={(e) => setMonth(Number(e.target.value))} />
          </label>
          <label>
            Desired money/day
            <input value={desiredMoneyPerDay} onChange={(e) => setDesiredMoneyPerDay(e.target.value)} />
          </label>
          <label>
            Period mode
            <select value={periodMode} onChange={(e) => setPeriodMode(e.target.value as 'RestOfMonth' | 'ThisAndNextMonth')}>
              <option value="RestOfMonth">Rest of month</option>
              <option value="ThisAndNextMonth">This + next month</option>
            </select>
          </label>
        </div>
        <button type="button" onClick={saveDemoDataAndForecast} disabled={loading}>
          {loading ? 'Calculating…' : 'Save month + calculate'}
        </button>

        {errorMessage && <p className="error">{errorMessage}</p>}

        {result && (
          <div className="result">
            <p><strong>Remaining budget:</strong> {result.remainingBudget}</p>
            <p><strong>Money/day (today):</strong> {result.moneyPerDay ?? 'N/A'}</p>
            <p><strong>Money/day (tomorrow):</strong> {result.moneyPerDayTomorrow ?? 'N/A'}</p>
            <p><strong>Possible savings:</strong> {result.possibleSavings ?? 'N/A'}</p>
            <p><strong>Days until goal:</strong> {result.daysUntilGoal ?? 'N/A'}</p>

            <h3>Emergency scenarios</h3>
            <ul>
              {result.scenarios.map((scenario) => (
                <li key={scenario.label}>
                  {scenario.label}: {scenario.moneyPerDayAfterScenario ?? 'N/A'} / day
                </li>
              ))}
            </ul>
          </div>
        )}
      </section>
    </div>
  );
}

export default App;
