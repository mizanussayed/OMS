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
    Task UpdateOrderStatusAsync(int orderId, DressOrderStatus status);
    Task UpdateClothRemainingMetersAsync(int clothId, double metersUsed);
}