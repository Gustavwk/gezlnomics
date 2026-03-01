namespace Backend.Domain;

public enum TransactionKind
{
    ExpenseActual = 0,
    ExpensePlanned = 1,
    SavingsTransferOut = 2,
    SavingsTransferIn = 3
}

public enum TransactionStatus
{
    Active = 0,
    Canceled = 1
}

public enum RecurringFrequency
{
    Weekly = 0,
    Monthly = 1,
    Yearly = 2
}
