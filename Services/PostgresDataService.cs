using Npgsql;
using OMS.Models;

namespace OMS.Services;

public class PostgresDataService : IDataService, IDisposable
{
    private readonly string _connectionString;
    private readonly NpgsqlDataSource? _dataSource;
    private readonly ISettingsService _settingsService;
    private bool _disposed;

    public PostgresDataService(string connectionString, ISettingsService settingsService, bool useConnectionPooling = true)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        if (useConnectionPooling)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            _dataSource = dataSourceBuilder.Build();
        }
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
    {
        NpgsqlConnection connection;
        
        if (_dataSource != null)
        {
            connection = await _dataSource.OpenConnectionAsync();
        }
        else
        {
            connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
        }
        
        return connection;
    }

    #region Cloths

    public async Task<IReadOnlyList<Cloth>> GetClothsAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var cloths = new List<Cloth>();

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, UNIQUE_CODE, NAME, COLOR, PRICE_PER_METER, TOTAL_METERS, 
                     REMAINING_METERS, ADDED_DATE
              FROM cloths 
              ORDER BY ADDED_DATE DESC",
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cloths.Add(new Cloth
            {
                Id = reader.GetInt32(0),
                UniqueCode = reader.GetString(1),
                Name = reader.GetString(2),
                Color = reader.GetString(3),
                PricePerMeter = (double)reader.GetDecimal(4),
                TotalMeters = (double)reader.GetDecimal(5),
                RemainingMeters = (double)reader.GetDecimal(6),
                AddedDate = reader.GetDateTime(7)
            });
        }

        return cloths;
    }

    public async Task AddClothAsync(Cloth cloth)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(cloth);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"INSERT INTO cloths (NAME, COLOR, PRICE_PER_METER, TOTAL_METERS, 
                                  REMAINING_METERS, ADDED_DATE)
              VALUES (@name, @color, @pricePerMeter, @totalMeters, 
                      @remainingMeters, @addedDate)
              RETURNING ID",
            connection);

        command.Parameters.AddWithValue("@name", cloth.Name);
        command.Parameters.AddWithValue("@color", cloth.Color);
        command.Parameters.AddWithValue("@pricePerMeter", (decimal)cloth.PricePerMeter);
        command.Parameters.AddWithValue("@totalMeters", (decimal)cloth.TotalMeters);
        command.Parameters.AddWithValue("@remainingMeters", (decimal)cloth.RemainingMeters);
        command.Parameters.AddWithValue("@addedDate", cloth.AddedDate);

        var id = await command.ExecuteScalarAsync();
        cloth.Id = Convert.ToInt32(id);
        
        // Auto-generate UniqueCode using settings prefix
        var prefix = await _settingsService.GetClothCodePrefixAsync();
        cloth.UniqueCode = $"{prefix}-{cloth.Id}";
        
        // Update the record with the generated UniqueCode
        await using var updateCommand = new NpgsqlCommand(
            "UPDATE cloths SET UNIQUE_CODE = @uniqueCode WHERE ID = @id",
            connection);
        
        updateCommand.Parameters.AddWithValue("@uniqueCode", cloth.UniqueCode);
        updateCommand.Parameters.AddWithValue("@id", cloth.Id);
        await updateCommand.ExecuteNonQueryAsync();
    }

    public async Task UpdateClothAsync(Cloth cloth)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(cloth);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"UPDATE cloths 
              SET UNIQUE_CODE = @uniqueCode, NAME = @name, COLOR = @color, 
                  PRICE_PER_METER = @pricePerMeter, TOTAL_METERS = @totalMeters, 
                  REMAINING_METERS = @remainingMeters, ADDED_DATE = @addedDate
              WHERE ID = @id",
            connection);

        command.Parameters.AddWithValue("@id", cloth.Id);
        command.Parameters.AddWithValue("@uniqueCode", cloth.UniqueCode);
        command.Parameters.AddWithValue("@name", cloth.Name);
        command.Parameters.AddWithValue("@color", cloth.Color);
        command.Parameters.AddWithValue("@pricePerMeter", (decimal)cloth.PricePerMeter);
        command.Parameters.AddWithValue("@totalMeters", (decimal)cloth.TotalMeters);
        command.Parameters.AddWithValue("@remainingMeters", (decimal)cloth.RemainingMeters);
        command.Parameters.AddWithValue("@addedDate", cloth.AddedDate);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Cloth with ID {cloth.Id} not found.");
        }
    }

    public async Task DeleteClothAsync(int clothId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Check if cloth is used in any orders
        if (await IsClothUsedInOrdersAsync(clothId))
        {
            throw new InvalidOperationException("Cannot delete cloth that is used in orders. Please delete the related orders first.");
        }

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            "DELETE FROM cloths WHERE ID = @id",
            connection);

        command.Parameters.AddWithValue("@id", clothId);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Cloth with ID {clothId} not found.");
        }
    }

    public async Task UpdateClothRemainingMetersAsync(int clothId, double metersUsed)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"UPDATE cloths 
              SET REMAINING_METERS = REMAINING_METERS - @metersUsed
              WHERE ID = @id AND REMAINING_METERS >= @metersUsed",
            connection);

        command.Parameters.AddWithValue("@id", clothId);
        command.Parameters.AddWithValue("@metersUsed", (decimal)metersUsed);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException("Insufficient cloth remaining or cloth not found.");
        }
    }

    #endregion

    #region Orders

    public async Task<IReadOnlyList<DressOrder>> GetOrdersAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var orders = new List<DressOrder>();

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, UNIQUE_CODE, CUSTOMER_NAME, MOBILE_NUMBER, DRESS_TYPE, 
                     CLOTH_ID, METERS_USED, STATUS, ASSIGNED_TO, ORDER_DATE
              FROM dress_orders 
              ORDER BY ORDER_DATE DESC",
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(new DressOrder
            {
                Id = reader.GetInt32(0),
                UniqueCode = reader.GetString(1),
                CustomerName = reader.GetString(2),
                MobileNumber = reader.GetString(3),
                DressType = reader.GetString(4),
                ClothId = reader.GetInt32(5),
                MetersUsed = (double)reader.GetDecimal(6),
                Status = Enum.Parse<DressOrderStatus>(reader.GetString(7)),
                AssignedTo = reader.GetInt32(8),
                OrderDate = reader.GetDateTime(9)
            });
        }

        return orders;
    }

    public async Task AddOrderAsync(DressOrder order)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(order);

        await using var connection = await GetConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Insert the order
            await using var command = new NpgsqlCommand(
                @"INSERT INTO dress_orders (CUSTOMER_NAME, MOBILE_NUMBER, 
                                           DRESS_TYPE, CLOTH_ID, METERS_USED, STATUS, 
                                           ASSIGNED_TO, ORDER_DATE)
                  VALUES (@customerName, @mobileNumber, @dressType, 
                          @clothId, @metersUsed, @status::dress_order_status, 
                          @assignedTo, @orderDate)
                  RETURNING ID",
                connection,
                transaction);

            command.Parameters.AddWithValue("@customerName", order.CustomerName);
            command.Parameters.AddWithValue("@mobileNumber", order.MobileNumber);
            command.Parameters.AddWithValue("@dressType", order.DressType);
            command.Parameters.AddWithValue("@clothId", order.ClothId);
            command.Parameters.AddWithValue("@metersUsed", (decimal)order.MetersUsed);
            command.Parameters.AddWithValue("@status", order.Status.ToString());
            command.Parameters.AddWithValue("@assignedTo", order.AssignedTo);
            command.Parameters.AddWithValue("@orderDate", order.OrderDate);

            var id = await command.ExecuteScalarAsync();
            order.Id = Convert.ToInt32(id);
            
            // Auto-generate UniqueCode using settings prefix
            var prefix = await _settingsService.GetOrderCodePrefixAsync();
            order.UniqueCode = $"{prefix}-{order.Id}";
            
            // Update the record with the generated UniqueCode
            await using var updateCommand = new NpgsqlCommand(
                "UPDATE dress_orders SET UNIQUE_CODE = @uniqueCode WHERE ID = @id",
                connection,
                transaction);
            
            updateCommand.Parameters.AddWithValue("@uniqueCode", order.UniqueCode);
            updateCommand.Parameters.AddWithValue("@id", order.Id);
            await updateCommand.ExecuteNonQueryAsync();

            // Update cloth remaining meters
            await using var updateClothCommand = new NpgsqlCommand(
                @"UPDATE cloths 
                  SET REMAINING_METERS = REMAINING_METERS - @metersUsed
                  WHERE ID = @clothId AND REMAINING_METERS >= @metersUsed",
                connection,
                transaction);

            updateClothCommand.Parameters.AddWithValue("@clothId", order.ClothId);
            updateClothCommand.Parameters.AddWithValue("@metersUsed", (decimal)order.MetersUsed);

            var rowsAffected = await updateClothCommand.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Insufficient cloth remaining.");
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateOrderAsync(DressOrder order)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(order);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"UPDATE dress_orders 
              SET UNIQUE_CODE = @uniqueCode, CUSTOMER_NAME = @customerName, 
                  MOBILE_NUMBER = @mobileNumber, DRESS_TYPE = @dressType, 
                  CLOTH_ID = @clothId, METERS_USED = @metersUsed, 
                  STATUS = @status::dress_order_status, ASSIGNED_TO = @assignedTo, 
                  ORDER_DATE = @orderDate
              WHERE ID = @id",
            connection);

        command.Parameters.AddWithValue("@id", order.Id);
        command.Parameters.AddWithValue("@uniqueCode", order.UniqueCode);
        command.Parameters.AddWithValue("@customerName", order.CustomerName);
        command.Parameters.AddWithValue("@mobileNumber", order.MobileNumber);
        command.Parameters.AddWithValue("@dressType", order.DressType);
        command.Parameters.AddWithValue("@clothId", order.ClothId);
        command.Parameters.AddWithValue("@metersUsed", (decimal)order.MetersUsed);
        command.Parameters.AddWithValue("@status", order.Status.ToString());
        command.Parameters.AddWithValue("@assignedTo", order.AssignedTo);
        command.Parameters.AddWithValue("@orderDate", order.OrderDate);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Order with ID {order.Id} not found.");
        }
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            "DELETE FROM dress_orders WHERE ID = @id",
            connection);

        command.Parameters.AddWithValue("@id", orderId);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Order with ID {orderId} not found.");
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId, DressOrderStatus status)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"UPDATE dress_orders 
              SET STATUS = @status::dress_order_status
              WHERE ID = @id",
            connection);

        command.Parameters.AddWithValue("@id", orderId);
        command.Parameters.AddWithValue("@status", status.ToString());

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Order with ID {orderId} not found.");
        }
    }

    #endregion

    #region Employees

    public async Task<IReadOnlyList<Employee>> GetEmployeesAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var employees = new List<Employee>();

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, NAME, USERNAME, PASSWORD, MOBILE_NUMBER
              FROM employees 
              ORDER BY NAME",
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            employees.Add(new Employee
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Username = reader.GetString(2),
                Password = reader.GetString(3),
                MobileNumber = reader.GetString(4)
            });
        }

        return employees;
    }

    public async Task AddEmployeeAsync(Employee employee)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(employee);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"INSERT INTO employees (NAME, USERNAME, PASSWORD, MOBILE_NUMBER)
              VALUES (@name, @username, @password, @mobileNumber)
              RETURNING ID",
            connection);

        command.Parameters.AddWithValue("@name", employee.Name);
        command.Parameters.AddWithValue("@username", employee.Username);
        command.Parameters.AddWithValue("@password", employee.Password);
        command.Parameters.AddWithValue("@mobileNumber", employee.MobileNumber);

        var id = await command.ExecuteScalarAsync();
        employee.Id = Convert.ToInt32(id);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Tests the database connection
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets database statistics
    /// </summary>
    public async Task<(int TotalCloths, int TotalOrders, int PendingOrders, int TotalEmployees)> GetDatabaseStatsAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT 
                (SELECT COUNT(*) FROM cloths) as total_cloths,
                (SELECT COUNT(*) FROM dress_orders) as total_orders,
                (SELECT COUNT(*) FROM dress_orders WHERE STATUS = 'Pending') as pending_orders,
                (SELECT COUNT(*) FROM employees) as total_employees",
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                Convert.ToInt32(reader.GetInt64(0)),
                Convert.ToInt32(reader.GetInt64(1)),
                Convert.ToInt32(reader.GetInt64(2)),
                Convert.ToInt32(reader.GetInt64(3))
            );
        }

        return (0, 0, 0, 0);
    }

    /// <summary>
    /// Gets cloth by ID
    /// </summary>
    public async Task<Cloth?> GetClothByIdAsync(int clothId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, UNIQUE_CODE, NAME, COLOR, PRICE_PER_METER, TOTAL_METERS, 
                     REMAINING_METERS, ADDED_DATE
              FROM cloths 
              WHERE ID = @id",
            connection);

        command.Parameters.AddWithValue("@id", clothId);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Cloth
            {
                Id = reader.GetInt32(0),
                UniqueCode = reader.GetString(1),
                Name = reader.GetString(2),
                Color = reader.GetString(3),
                PricePerMeter = (double)reader.GetDecimal(4),
                TotalMeters = (double)reader.GetDecimal(5),
                RemainingMeters = (double)reader.GetDecimal(6),
                AddedDate = reader.GetDateTime(7)
            };
        }

        return null;
    }

    /// <summary>
    /// Gets order by ID
    /// </summary>
    public async Task<DressOrder?> GetOrderByIdAsync(int orderId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, UNIQUE_CODE, CUSTOMER_NAME, MOBILE_NUMBER, DRESS_TYPE, 
                     CLOTH_ID, METERS_USED, STATUS, ASSIGNED_TO, ORDER_DATE
              FROM dress_orders 
              WHERE ID = @id",
            connection);

        command.Parameters.AddWithValue("@id", orderId);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DressOrder
            {
                Id = reader.GetInt32(0),
                UniqueCode = reader.GetString(1),
                CustomerName = reader.GetString(2),
                MobileNumber = reader.GetString(3),
                DressType = reader.GetString(4),
                ClothId = reader.GetInt32(5),
                MetersUsed = (double)reader.GetDecimal(6),
                Status = Enum.Parse<DressOrderStatus>(reader.GetString(7)),
                AssignedTo = reader.GetInt32(8),
                OrderDate = reader.GetDateTime(9)
            };
        }

        return null;
    }

    /// <summary>
    /// Gets orders by status
    /// </summary>
    public async Task<IReadOnlyList<DressOrder>> GetOrdersByStatusAsync(DressOrderStatus status)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var orders = new List<DressOrder>();

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, UNIQUE_CODE, CUSTOMER_NAME, MOBILE_NUMBER, DRESS_TYPE, 
                     CLOTH_ID, METERS_USED, STATUS, ASSIGNED_TO, ORDER_DATE
              FROM dress_orders 
              WHERE STATUS = @status::dress_order_status
              ORDER BY ORDER_DATE DESC",
            connection);

        command.Parameters.AddWithValue("@status", status.ToString());

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(new DressOrder
            {
                Id = reader.GetInt32(0),
                UniqueCode = reader.GetString(1),
                CustomerName = reader.GetString(2),
                MobileNumber = reader.GetString(3),
                DressType = reader.GetString(4),
                ClothId = reader.GetInt32(5),
                MetersUsed = (double)reader.GetDecimal(6),
                Status = Enum.Parse<DressOrderStatus>(reader.GetString(7)),
                AssignedTo = reader.GetInt32(8),
                OrderDate = reader.GetDateTime(9)
            });
        }

        return orders;
    }

    /// <summary>
    /// Gets orders assigned to an employee
    /// </summary>
    public async Task<IReadOnlyList<DressOrder>> GetOrdersByEmployeeAsync(int employeeId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var orders = new List<DressOrder>();

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, UNIQUE_CODE, CUSTOMER_NAME, MOBILE_NUMBER, DRESS_TYPE, 
                     CLOTH_ID, METERS_USED, STATUS, ASSIGNED_TO, ORDER_DATE
              FROM dress_orders 
              WHERE ASSIGNED_TO = @employeeId
              ORDER BY ORDER_DATE DESC",
            connection);

        command.Parameters.AddWithValue("@employeeId", employeeId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(new DressOrder
            {
                Id = reader.GetInt32(0),
                UniqueCode = reader.GetString(1),
                CustomerName = reader.GetString(2),
                MobileNumber = reader.GetString(3),
                DressType = reader.GetString(4),
                ClothId = reader.GetInt32(5),
                MetersUsed = (double)reader.GetDecimal(6),
                Status = Enum.Parse<DressOrderStatus>(reader.GetString(7)),
                AssignedTo = reader.GetInt32(8),
                OrderDate = reader.GetDateTime(9)
            });
        }

        return orders;
    }
    
    /// <summary>
    /// Checks if a cloth is used in any orders
    /// </summary>
    public async Task<bool> IsClothUsedInOrdersAsync(int clothId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            "SELECT EXISTS(SELECT 1 FROM dress_orders WHERE CLOTH_ID = @clothId)",
            connection);

        command.Parameters.AddWithValue("@clothId", clothId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToBoolean(result);
    }
    
    /// <summary>
    /// Gets top N cloths with lowest stock (by percentage)
    /// </summary>
    public async Task<IReadOnlyList<Cloth>> GetLowStockClothsAsync(int count = 5)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var cloths = new List<Cloth>();

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, UNIQUE_CODE, NAME, COLOR, PRICE_PER_METER, TOTAL_METERS, 
                     REMAINING_METERS, ADDED_DATE
              FROM cloths 
              WHERE TOTAL_METERS > 0
              ORDER BY (REMAINING_METERS / TOTAL_METERS) ASC
              LIMIT @count",
            connection);

        command.Parameters.AddWithValue("@count", count);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cloths.Add(new Cloth
            {
                Id = reader.GetInt32(0),
                UniqueCode = reader.GetString(1),
                Name = reader.GetString(2),
                Color = reader.GetString(3),
                PricePerMeter = (double)reader.GetDecimal(4),
                TotalMeters = (double)reader.GetDecimal(5),
                RemainingMeters = (double)reader.GetDecimal(6),
                AddedDate = reader.GetDateTime(7)
            });
        }

        return cloths;
    }
    
    /// <summary>
    /// Gets top N latest cloths
    /// </summary>
    public async Task<IReadOnlyList<Cloth>> GetLatestClothsAsync(int count = 5)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var cloths = new List<Cloth>();

        await using var connection = await GetConnectionAsync();

        await using var command = new NpgsqlCommand(
            @"SELECT ID, UNIQUE_CODE, NAME, COLOR, PRICE_PER_METER, TOTAL_METERS, 
                     REMAINING_METERS, ADDED_DATE
              FROM cloths 
              ORDER BY ADDED_DATE DESC
              LIMIT @count",
            connection);

        command.Parameters.AddWithValue("@count", count);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cloths.Add(new Cloth
            {
                Id = reader.GetInt32(0),
                UniqueCode = reader.GetString(1),
                Name = reader.GetString(2),
                Color = reader.GetString(3),
                PricePerMeter = (double)reader.GetDecimal(4),
                TotalMeters = (double)reader.GetDecimal(5),
                RemainingMeters = (double)reader.GetDecimal(6),
                AddedDate = reader.GetDateTime(7)
            });
        }

        return cloths;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dataSource?.Dispose();
            }

            _disposed = true;
        }
    }

    #endregion
}
