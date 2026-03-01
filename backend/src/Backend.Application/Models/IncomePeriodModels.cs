namespace Backend.Application.Models;

public sealed record IncomePeriodDto(Guid Id, DateOnly PeriodStartDate, DateOnly PeriodEndDate, decimal StartingBalance, DateTime CreatedAt);
public sealed record UpsertIncomePeriodRequest(DateOnly PeriodStartDate, DateOnly PeriodEndDate, decimal StartingBalance);
