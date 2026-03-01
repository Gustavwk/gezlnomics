import type {
  AuthUser,
  CreateRecurringRulePayload,
  CreateTransactionPayload,
  IncomePeriod,
  LedgerSummary,
  RecurringRule,
  Transaction
} from '../types/models';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string;

async function request<T>(path: string, init?: RequestInit): Promise<T> {
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

export const apiClient = {
  auth: {
    me: () => request<AuthUser>('/api/auth/me'),
    login: (email: string, password: string) =>
      request<AuthUser>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password })
      }),
    signup: (email: string, password: string) =>
      request<AuthUser>('/api/auth/signup', {
        method: 'POST',
        body: JSON.stringify({ email, password })
      }),
    logout: () => request<void>('/api/auth/logout', { method: 'POST' })
  },
  ledger: {
    summary: () => request<LedgerSummary>('/api/ledger/summary')
  },
  incomePeriods: {
    list: () => request<IncomePeriod[]>('/api/income-periods/'),
    create: (payload: { periodStartDate: string; periodEndDate: string; startingBalance: number }) =>
      request<IncomePeriod>('/api/income-periods/', {
        method: 'POST',
        body: JSON.stringify(payload)
      }),
    update: (id: string, payload: { periodStartDate: string; periodEndDate: string; startingBalance: number }) =>
      request<IncomePeriod>(`/api/income-periods/${id}`, {
        method: 'PUT',
        body: JSON.stringify(payload)
      })
  },
  transactions: {
    list: () => request<Transaction[]>('/api/transactions/'),
    create: (payload: CreateTransactionPayload) =>
      request<Transaction>('/api/transactions/', {
        method: 'POST',
        body: JSON.stringify(payload)
      }),
    remove: (id: string) => request<void>(`/api/transactions/${id}`, { method: 'DELETE' })
  },
  recurring: {
    list: () => request<RecurringRule[]>('/api/recurring-rules/'),
    create: (payload: CreateRecurringRulePayload) =>
      request<RecurringRule>('/api/recurring-rules/', {
        method: 'POST',
        body: JSON.stringify(payload)
      }),
    remove: (id: string) => request<void>(`/api/recurring-rules/${id}`, { method: 'DELETE' })
  }
};
