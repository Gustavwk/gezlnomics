import { FormEvent } from 'react';
import type { LedgerSummary } from '../../types/models';

type Props = {
  summary: LedgerSummary;
  startSaldo: string;
  periodeValg: { key: string; label: string }[];
  valgtPeriodeNoegle: string;
  setStartSaldo: (value: string) => void;
  onSaveStartsaldo: () => Promise<void>;
  onVaelgPeriode: (noegle: string) => Promise<void>;
  onOpretNaestePeriode: () => Promise<void>;
};

export function LedgerPanel({
  summary,
  startSaldo,
  periodeValg,
  valgtPeriodeNoegle,
  setStartSaldo,
  onSaveStartsaldo,
  onVaelgPeriode,
  onOpretNaestePeriode
}: Props) {
  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onSaveStartsaldo();
  }

  return (
    <section className="panel">
      <h2>Lønperiode</h2>

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

      <p>
        {summary.periodStart} til {summary.periodEnd}
      </p>

      <div className="ledger-controls">
        <div className="periode-toolbar">
          <label>
            Måned/periode
            <select value={valgtPeriodeNoegle} onChange={(e) => void onVaelgPeriode(e.target.value)}>
              {periodeValg.map((p) => (
                <option key={p.key} value={p.key}>
                  {p.label}
                </option>
              ))}
            </select>
          </label>
          <button type="button" className="align-end btn-small" onClick={() => void onOpretNaestePeriode()}>
            Opret næste periode
          </button>
        </div>

        <form onSubmit={handleSubmit} className="startsaldo-form">
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
          <button type="submit" className="align-end btn-small">Gem startsaldo</button>
        </form>
      </div>
    </section>
  );
}
