using OMS.Models;

namespace OMS.Services;

public interface IDataService
{
    Task<IReadOnlyList<Cloth>> GetClothsAsync();
    Task<IReadOnlyList<DressOrder>> GetOrdersAsync();
    Task AddClothAsync(Cloth cloth);
    Task AddOrderAsync(DressOrder order);
    Task UpdateOrderStatusAsync(string orderId, DressOrderStatus status);
}

public class MockDataService : IDataService
{
    private List<Cloth> _cloths = new()
    {
        new("1", "Cotton Premium", "Blue", 450, 50, 38, DateTime.Now.AddDays(-10)),
        new("2", "Silk Blend", "Red", 850, 30, 25, DateTime.Now.AddDays(-5)),
        new("3", "Linen Light", "Green", 350, 40, 40, DateTime.Now.AddDays(-2)),
    };

    private List<DressOrder> _orders = new()
    {
        new("1", "Priya Sharma", "Kurti", "1", 2.5m, DressOrderStatus.Pending, "maker-1", DateTime.Now.AddDays(-1)),
        new("2", "Anita Desai", "Saree Blouse", "2", 1.5m, DressOrderStatus.Completed, "maker-1", DateTime.Now.AddDays(-3)),
        new("3", "Rahul Mehta", "Shirt", "1", 2.0m, DressOrderStatus.Pending, "maker-2", DateTime.Now.AddDays(-2)),
    };

    public Task<IReadOnlyList<Cloth>> GetClothsAsync() => Task.FromResult<IReadOnlyList<Cloth>>(_cloths);

    public Task<IReadOnlyList<DressOrder>> GetOrdersAsync() => Task.FromResult<IReadOnlyList<DressOrder>>(_orders);

    public Task AddClothAsync(Cloth cloth)
    {
        cloth = cloth with { Id = Guid.NewGuid().ToString() };
        _cloths.Add(cloth);
        return Task.CompletedTask;
    }

    public Task AddOrderAsync(DressOrder order)
    {
        order = order with { Id = Guid.NewGuid().ToString() };
        _orders.Add(order);
        var cloth = _cloths.FirstOrDefault(c => c.Id == order.ClothId);
        if (cloth != null)
        {
            _cloths[_cloths.IndexOf(cloth)] = cloth with { RemainingMeters = cloth.RemainingMeters - order.MetersUsed };
        }
        return Task.CompletedTask;
    }

    public Task UpdateOrderStatusAsync(string orderId, DressOrderStatus status)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order != null)
        {
            _orders[_orders.IndexOf(order)] = order with { Status = status };
        }
        return Task.CompletedTask;
    }
}