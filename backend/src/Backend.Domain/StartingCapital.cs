namespace Backend.Domain;

public sealed class StartingCapital
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreatedAt { get; set; }
}
