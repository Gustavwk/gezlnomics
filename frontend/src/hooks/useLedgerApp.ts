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

export function useLedgerApp() {
  const [bruger, setBruger] = useState<AuthUser | null>(null);
  const [indlaeser, setIndlaeser] = useState(true);
  const [fejl, setFejl] = useState('');

  const [summary, setSummary] = useState<LedgerSummary | null>(null);
  const [perioder, setPerioder] = useState<IncomePeriod[]>([]);
  const [transaktioner, setTransaktioner] = useState<Transaction[]>([]);
  const [fasteUdgifter, setFasteUdgifter] = useState<RecurringRule[]>([]);
  const [startSaldo, setStartSaldo] = useState('0');

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

  async function hentAlt() {
    const [s, p, t, f] = await Promise.all([
      apiClient.ledger.summary(),
      apiClient.incomePeriods.list(),
      apiClient.transactions.list(),
      apiClient.recurring.list()
    ]);

    setSummary(s);
    setPerioder(p);
    setTransaktioner(t);
    setFasteUdgifter(f);

    const match = p.find((x) => x.periodStartDate === s.periodStart && x.periodEndDate === s.periodEnd);
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

  async function handleAuth(mode: AuthMode, email: string, password: string) {
    setFejl('');
    try {
      const me =
        mode === 'signup'
          ? await apiClient.auth.signup(email, password)
          : await apiClient.auth.login(email, password);

      setBruger(me);
      await hentAlt();
    } catch {
      setFejl('Login/oprettelse fejlede.');
    }
  }

  async function handleLogout() {
    await apiClient.auth.logout();
    setBruger(null);
    setSummary(null);
    setPerioder([]);
    setTransaktioner([]);
    setFasteUdgifter([]);
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

  return {
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
  };
}
