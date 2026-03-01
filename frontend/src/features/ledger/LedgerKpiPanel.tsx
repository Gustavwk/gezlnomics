import { MoneyPerDayThreshold } from '../../constants/kpi';
import type { LedgerSummary } from '../../types/models';

type Props = {
  summary: LedgerSummary;
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

export function LedgerKpiPanel({ summary }: Props) {
  const iDag = new Date().toISOString().slice(0, 10);
  const naesteLoendag = addDays(summary.periodEnd, 1);
  const erFremtidigPeriode = summary.periodStart > iDag;

  const visteDageTilNaesteLoen = erFremtidigPeriode ? diffDays(iDag, naesteLoendag) : summary.daysUntilNextPayday;
  const vistePengePrDag = erFremtidigPeriode
    ? Math.round((Math.max(0, summary.forecastBalance) / visteDageTilNaesteLoen) * 100) / 100
    : summary.moneyPerDayStartOfDay;
  const vistePengePrDagFremad = erFremtidigPeriode ? vistePengePrDag : summary.moneyPerDay;

  return (
    <section className="panel">
      <h2>Overblik</h2>
      <div className="kpi">
        <article className={summary.currentBalance < 0 ? 'kpi-bad' : 'kpi-ok'}>
          <h3>Nuværende saldo</h3>
          <strong>
            {summary.currentBalance.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article className={moneyPerDayKpiClass(vistePengePrDag)}>
          <h3>Penge pr. dag (før i dag)</h3>
          <strong>
            {vistePengePrDag.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article className={moneyPerDayKpiClass(vistePengePrDagFremad)}>
          <h3>Penge/dag frem</h3>
          <strong>
            {vistePengePrDagFremad.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article className={summary.spentTodayExcludingRecurring > 0 ? 'kpi-warn' : 'kpi-ok'}>
          <h3>Forbrug i dag (uden faste)</h3>
          <strong>
            {summary.spentTodayExcludingRecurring.toFixed(2)} {summary.currencyCode}
          </strong>
        </article>
        <article className={visteDageTilNaesteLoen <= 3 ? 'kpi-warn' : 'kpi-ok'}>
          <h3>Dage til næste løn</h3>
          <strong>{visteDageTilNaesteLoen}</strong>
        </article>
      </div>
    </section>
  );
}
