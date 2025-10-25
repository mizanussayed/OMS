namespace OMS.Models;

public record DressOrder(
    string Id,
    string CustomerName,
    string DressType,
    string ClothId,
    decimal MetersUsed,
    DressOrderStatus Status,
    string AssignedTo,
    DateTime OrderDate
);

public enum DressOrderStatus
{
    Pending,
    Completed,
    Delivered
}