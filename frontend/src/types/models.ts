export type AuthUser = { id: string; email: string };

export type LedgerSummary = {
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

export type IncomePeriod = {
  id: string;
  periodStartDate: string;
  periodEndDate: string;
  startingBalance: number;
};

export type Transaction = {
  id: string;
  date: string;
  amount: number;
  category: string;
  note?: string;
  kind: number;
};

export type RecurringRule = {
  id: string;
  title: string;
  amount: number;
  category: string;
  note?: string;
  ruleKind: number;
  frequency: number;
  startDate: string;
  endDate?: string;
};

export type AuthMode = 'login' | 'signup';

export type CreateTransactionPayload = {
  date: string;
  amount: number;
  category: string;
  note: string | null;
  kind: number;
  status: number;
};

export type CreateRecurringRulePayload = {
  title: string;
  amount: number;
  category: string;
  note: string | null;
  ruleKind: number;
  frequency: number;
  startDate: string;
  endDate: string | null;
  isActive: boolean;
};
