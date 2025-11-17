using OMS.Models;

namespace OMS.Services;

public interface IDataService
{
    Task<IReadOnlyList<Cloth>> GetClothsAsync();
    Task<IReadOnlyList<DressOrder>> GetOrdersAsync();
    Task<IReadOnlyList<Employee>> GetEmployeesAsync();
    Task AddClothAsync(Cloth cloth);
    Task AddOrderAsync(DressOrder order);
    Task AddEmployeeAsync(Employee employee);
    Task UpdateClothAsync(Cloth cloth);
    Task UpdateOrderAsync(DressOrder order);
    Task DeleteClothAsync(int clothId);
    Task DeleteOrderAsync(int orderId);
    Task UpdateOrderStatusAsync(int orderId, DressOrderStatus status);
    Task UpdateClothRemainingMetersAsync(int clothId, double metersUsed);
}