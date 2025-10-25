namespace OMS.Models;

public record Cloth(
    string Id,
    string Name,
    string Color,
    decimal PricePerMeter,
    decimal TotalMeters,
    decimal RemainingMeters,
    DateTime AddedDate
);