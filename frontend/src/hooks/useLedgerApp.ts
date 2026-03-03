import { useEffect, useMemo, useState } from 'react';
import { apiClient } from '../api/client';
import type {
  AuthMode,
  AuthUser,
  CreateRecurringRulePayload,
  CreateTransactionPayload,
  IncomePeriod,
  LedgerSummary,
  RecurringRule,
  Transaction
} from '../types/models';

type PeriodeVindue = {
  periodStart: string;
  periodEnd: string;
};

function periodeNoegle(periodStart: string, periodEnd: string): string {
  return `${periodStart}|${periodEnd}`;
}

function parseNoegle(noegle: string): PeriodeVindue | null {
  const parts = noegle.split('|');
  if (parts.length !== 2 || !parts[0] || !parts[1]) {
    return null;
  }

  return { periodStart: parts[0], periodEnd: parts[1] };
}

function addDays(isoDate: string, days: number): string {
  const date = new Date(`${isoDate}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + days);
  return date.toISOString().slice(0, 10);
}

function addMonths(isoDate: string, months: number): string {
  const date = new Date(`${isoDate}T00:00:00Z`);
  date.setUTCMonth(date.getUTCMonth() + months);
  return date.toISOString().slice(0, 10);
}

export function useLedgerApp() {
  const [bruger, setBruger] = useState<AuthUser | null>(null);
  const [indlaeser, setIndlaeser] = useState(true);
  const [fejl, setFejl] = useState('');

  const [summary, setSummary] = useState<LedgerSummary | null>(null);
  const [perioder, setPerioder] = useState<IncomePeriod[]>([]);
  const [transaktioner, setTransaktioner] = useState<Transaction[]>([]);
  const [fasteUdgifter, setFasteUdgifter] = useState<RecurringRule[]>([]);
  const [startSaldo, setStartSaldo] = useState('0');
  const [valgtPeriode, setValgtPeriode] = useState<PeriodeVindue | null>(null);

  const aktivPeriode = useMemo(() => {
    if (!summary) {
      return null;
    }

    return (
      perioder.find(
        (p) => p.periodStartDate === summary.periodStart && p.periodEndDate === summary.periodEnd
      ) ?? null
    );
  }, [summary, perioder]);

  const periodeValg = useMemo(() => {
    const fraGemte = perioder
      .map((p) => ({
        key: periodeNoegle(p.periodStartDate, p.periodEndDate),
        periodStart: p.periodStartDate,
        periodEnd: p.periodEndDate,
        label: `${p.periodStartDate} til ${p.periodEndDate}`
      }))
      .sort((a, b) => b.periodStart.localeCompare(a.periodStart));

    if (!summary) {
      return fraGemte;
    }

    const aktivKey = periodeNoegle(summary.periodStart, summary.periodEnd);
    if (fraGemte.some((p) => p.key === aktivKey)) {
      return fraGemte;
    }

    return [
      {
        key: aktivKey,
        periodStart: summary.periodStart,
        periodEnd: summary.periodEnd,
        label: `${summary.periodStart} til ${summary.periodEnd} (nuværende)`
      },
      ...fraGemte
    ];
  }, [perioder, summary]);

  async function hentAlt(foretrukken?: PeriodeVindue | null) {
    const [nuvSummary, p, f] = await Promise.all([
      apiClient.ledger.summary(),
      apiClient.incomePeriods.list(),
      apiClient.recurring.list()
    ]);

    const sorteretPerioder = [...p].sort((a, b) => b.periodStartDate.localeCompare(a.periodStartDate));
    setPerioder(sorteretPerioder);
    setFasteUdgifter(f);

    const valgt =
      foretrukken ??
      valgtPeriode ??
      {
        periodStart: nuvSummary.periodStart,
        periodEnd: nuvSummary.periodEnd
      };

    const iDag = new Date().toISOString().slice(0, 10);
    const asOf = iDag < valgt.periodStart ? valgt.periodStart : iDag > valgt.periodEnd ? valgt.periodEnd : iDag;

    const [s, t] = await Promise.all([
      apiClient.ledger.summary(asOf),
      apiClient.transactions.list({ from: valgt.periodStart, to: valgt.periodEnd })
    ]);

    setValgtPeriode({ periodStart: s.periodStart, periodEnd: s.periodEnd });
    setSummary(s);
    setTransaktioner(t);

    const match = sorteretPerioder.find((x) => x.periodStartDate === s.periodStart && x.periodEndDate === s.periodEnd);
    setStartSaldo((match?.startingBalance ?? 0).toString());
  }

  async function opdater() {
    setFejl('');
    try {
      await hentAlt();
    } catch {
      setFejl('Kunne ikke hente data.');
    }
  }

  useEffect(() => {
    (async () => {
      try {
        const me = await apiClient.auth.me();
        setBruger(me);
        await hentAlt();
      } catch {
        setBruger(null);
      } finally {
        setIndlaeser(false);
      }
    })();
  }, []);

  async function handleAuth(mode: AuthMode, username: string, password: string) {
    setFejl('');
    try {
      const me =
        mode === 'signup'
          ? await apiClient.auth.signup(username, password)
          : await apiClient.auth.login(username, password);

      setBruger(me);
      await hentAlt();
    } catch {
      setFejl('Login/oprettelse fejlede.');
    }
  }

  async function handleLogout() {
    try {
      await apiClient.auth.logout();
    } finally {
      setBruger(null);
      setSummary(null);
      setPerioder([]);
      setTransaktioner([]);
      setFasteUdgifter([]);
      setValgtPeriode(null);
    }
  }

  async function gemStartsaldo() {
    if (!summary) {
      return;
    }

    const payload = {
      periodStartDate: summary.periodStart,
      periodEndDate: summary.periodEnd,
      startingBalance: Number(startSaldo)
    };

    if (aktivPeriode) {
      await apiClient.incomePeriods.update(aktivPeriode.id, payload);
    } else {
      await apiClient.incomePeriods.create(payload);
    }

    await opdater();
  }

  async function opretTransaktion(payload: CreateTransactionPayload) {
    await apiClient.transactions.create(payload);
    await opdater();
  }

  async function sletTransaktion(id: string) {
    await apiClient.transactions.remove(id);
    await opdater();
  }

  async function opretFastUdgift(payload: CreateRecurringRulePayload) {
    await apiClient.recurring.create(payload);
    await opdater();
  }

  async function sletFastUdgift(id: string) {
    await apiClient.recurring.remove(id);
    await opdater();
  }

  async function vaelgPeriode(noegle: string) {
    setFejl('');
    const parsed = parseNoegle(noegle);
    if (!parsed) {
      return;
    }

    try {
      await hentAlt(parsed);
    } catch {
      setFejl('Kunne ikke skifte periode.');
    }
  }

  async function opretNaestePeriode() {
    if (!summary) {
      return;
    }

    setFejl('');

    try {
      const naesteStart = addDays(summary.periodEnd, 1);
      const naesteSlut = addDays(addMonths(naesteStart, 1), -1);
      const senesteKendteStartsaldo = perioder.length > 0 ? perioder[0].startingBalance : 0;
      const startSaldoNyPeriode = aktivPeriode?.startingBalance ?? senesteKendteStartsaldo;

      await apiClient.incomePeriods.create({
        periodStartDate: naesteStart,
        periodEndDate: naesteSlut,
        startingBalance: startSaldoNyPeriode
      });

      await hentAlt({ periodStart: naesteStart, periodEnd: naesteSlut });
    } catch {
      setFejl('Kunne ikke oprette ny periode.');
    }
  }

  return {
    bruger,
    indlaeser,
    fejl,
    summary,
    transaktioner,
    fasteUdgifter,
    startSaldo,
    periodeValg,
    valgtPeriodeNoegle: summary ? periodeNoegle(summary.periodStart, summary.periodEnd) : '',
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
  };
}
