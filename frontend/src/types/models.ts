import type { components } from '../generated/api-types';

type Schemas = components['schemas'];

export type AuthUser = Schemas['AuthUserDto'];
export type LedgerSummary = Schemas['LedgerSummaryDto'];
export type IncomePeriod = Schemas['IncomePeriodDto'];
export type Transaction = Schemas['TransactionDto'];
export type RecurringRule = Schemas['RecurringRuleDto'];

export type AuthMode = 'login' | 'signup';

export type CreateTransactionPayload = Schemas['UpsertTransactionRequest'];
export type CreateRecurringRulePayload = Schemas['UpsertRecurringRuleRequest'];
