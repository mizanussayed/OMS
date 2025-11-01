namespace OMS.Models;

public sealed class Cloth {
    public int Id { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public double PricePerMeter { get; set; }
    public double TotalMeters { get; set; }
    public double RemainingMeters { get; set; }
    public DateTime AddedDate { get; set; }
}