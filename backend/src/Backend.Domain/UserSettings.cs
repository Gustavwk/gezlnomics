namespace Backend.Domain;

public sealed class UserSettings
{
    public Guid UserId { get; set; }
    public int PaydayDayOfMonth { get; set; } = 1;
    public string CurrencyCode { get; set; } = "DKK";
    public string Timezone { get; set; } = "Europe/Copenhagen";
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
}
