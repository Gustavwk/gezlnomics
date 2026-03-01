import { FormEvent } from 'react';
import { MoneyPerDayThreshold } from '../../constants/kpi';
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

function addDays(isoDate: string, days: number): string {
  const date = new Date(`${isoDate}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + days);
  return date.toISOString().slice(0, 10);
}

function diffDays(fromIso: string, toIso: string): number {
  const from = new Date(`${fromIso}T00:00:00Z`).getTime();
  const to = new Date(`${toIso}T00:00:00Z`).getTime();
  return Math.max(1, Math.ceil((to - from) / (1000 * 60 * 60 * 24)));
}

function moneyPerDayKpiClass(value: number): string {
  if (value < MoneyPerDayThreshold.Bad) {
    return 'kpi-bad';
  }

  if (value < MoneyPerDayThreshold.Warn) {
    return 'kpi-warn';
  }

  if (value < MoneyPerDayThreshold.Ok) {
    return 'kpi-ok';
  }

  return 'kpi-good';
}

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

  const iDag = new Date().toISOString().slice(0, 10);
  const naesteLoendag = addDays(summary.periodEnd, 1);
  const erFremtidigPeriode = summary.periodStart > iDag;

  const visteDageTilNaesteLoen = erFremtidigPeriode ? diffDays(iDag, naesteLoendag) : summary.daysUntilNextPayday;
  const vistePengePrDag = erFremtidigPeriode
    ? Math.round((Math.max(0, summary.forecastBalance) / visteDageTilNaesteLoen) * 100) / 100
    : summary.moneyPerDay;

  return (
    <section className="panel">
      <h2>Lønperiode</h2>

      <div className="kpi">
        <article className={summary.currentBalance < 0 ? 'kpi-bad' : 'kpi-ok'}>
          <h3>Nuværende saldo</h3>
          <strong>
            {summary.currentBalance.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article className={moneyPerDayKpiClass(vistePengePrDag)}>
          <h3>Penge pr. dag</h3>
          <strong>
            {vistePengePrDag.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article className={visteDageTilNaesteLoen <= 3 ? 'kpi-warn' : 'kpi-ok'}>
          <h3>Dage til næste løn</h3>
          <strong>{visteDageTilNaesteLoen}</strong>
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
