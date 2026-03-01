import { FormEvent } from 'react';
import type { LedgerSummary } from '../../types/models';

type Props = {
  summary: LedgerSummary;
  startSaldo: string;
  setStartSaldo: (value: string) => void;
  onSaveStartsaldo: () => Promise<void>;
};

export function LedgerPanel({ summary, startSaldo, setStartSaldo, onSaveStartsaldo }: Props) {
  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onSaveStartsaldo();
  }

  return (
    <section className="panel">
      <h2>Lønperiode</h2>
      <p>
        {summary.periodStart} til {summary.periodEnd}
      </p>

      <div className="kpi">
        <article>
          <h3>Nuværende saldo</h3>
          <strong>
            {summary.currentBalance.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article>
          <h3>Prognose saldo</h3>
          <strong>
            {summary.forecastBalance.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article>
          <h3>Penge pr. dag</h3>
          <strong>
            {summary.moneyPerDay.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article>
          <h3>Dage til næste løn</h3>
          <strong>{summary.daysUntilNextPayday}</strong>
        </article>
      </div>

      <form onSubmit={handleSubmit} className="row wrap">
        <label>
          Startsaldo for denne lønperiode
          <input
            type="number"
            step="0.01"
            value={startSaldo}
            onChange={(e) => setStartSaldo(e.target.value)}
            required
          />
        </label>
        <button type="submit">Gem startsaldo</button>
      </form>
    </section>
  );
}
