namespace OMS.Models;

public sealed class DressOrder {
    public int Id { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string DressType { get; set; } = string.Empty;
    public int ClothId { get; set; }
    public double MetersUsed { get; set; }
    public DressOrderStatus Status { get; set; }
    public int AssignedTo { get; set; } 
    public DateTime OrderDate { get; set; }
}

public enum DressOrderStatus
{
    Pending,
    Completed,
    Delivered
}