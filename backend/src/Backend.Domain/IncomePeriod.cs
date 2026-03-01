namespace Backend.Domain;

public sealed class IncomePeriod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public decimal StartingBalance { get; set; }
    public DateTime CreatedAt { get; set; }
}
