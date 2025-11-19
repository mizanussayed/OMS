//using Google.Apis.Auth.OAuth2;
//using Google.Cloud.Firestore;
//using OMS.Models;

//namespace OMS.Services;

//public class FirebaseDataService : IDataService
//{
//    private const string OrdersCollection = "DressOrders";
//    private const string ClothCollection = "Cloths";
//    private const string EmployeesCollection = "Employees";
//    private const string CountersDoc = "OMSMeta/Counter";
//    private static readonly Lazy<Task<FirestoreDb>> _firestoreDb = new(() => CreateDb());
//    private static Task<FirestoreDb> GetDbAsync() => _firestoreDb.Value;
//    private static async Task<FirestoreDb> CreateDb()
//    {
//        try
//        {
//            await using var stream = await FileSystem.OpenAppPackageFileAsync("adminsdk.json");
//            using var reader = new StreamReader(stream);
//            var json = await reader.ReadToEndAsync();

//            var serviceCredential = CredentialFactory.FromJson<ServiceAccountCredential>(json);
//            var credential = serviceCredential.ToGoogleCredential();

//            var db = new FirestoreDbBuilder
//            {
//                GoogleCredential = credential,
//                ProjectId = "mypm-tailor-ffd0f",
//            }.Build();

//            return db;
//        }
//        catch
//        {
//            return null!;
//        }
//    }

//    public async Task<IReadOnlyList<Cloth>> GetClothsAsync()
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var q = db.Collection(ClothCollection).OrderByDescending(nameof(Cloth.Id));
//            var snap = await q.GetSnapshotAsync();
//            var cloths = new List<Cloth>(snap.Count);
//            foreach (var doc in snap.Documents)
//                cloths.Add(FromClothDoc(doc.ToDictionary()));
//            return cloths;
//        }
//        catch
//        {
//            return [];
//        }
//     }

//    public async Task<IReadOnlyList<DressOrder>> GetOrdersAsync()
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var q = db.Collection(OrdersCollection).OrderByDescending(nameof(DressOrder.Id));
//            var snap = await q.GetSnapshotAsync();
//            var orders = new List<DressOrder>(snap.Count);
//            foreach (var doc in snap.Documents)
//                orders.Add(FromOrderDoc(doc.ToDictionary()));
//            return orders;
//        }
//        catch
//        {
//            return [];
//        }
//    }
//    public async Task<IReadOnlyList<Employee>> GetEmployeesAsync()
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var q = db.Collection(EmployeesCollection).OrderByDescending(nameof(Employee.Id));
//            var snap = await q.GetSnapshotAsync();
//            var employees = new List<Employee>(snap.Count);
//            foreach (var doc in snap.Documents)
//                employees.Add(FromEmployee(doc.ToDictionary()));
//            return employees;
//        }
//        catch
//        {
//            return [];
//        }
//    }

//    public async Task AddClothAsync(Cloth cloth)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            cloth.Id = await GetNextClothIdAsync(db);
//            var docRef = db.Collection(ClothCollection).Document(cloth.Id.ToString());
//            await docRef.SetAsync(ToClothDoc(cloth));
//        }
//        catch
//        {
//            return;
//        }
//    }

//    public async Task AddOrderAsync(DressOrder order)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            order.Id = await GetNextOrderIdAsync(db);
//            var docRef = db.Collection(OrdersCollection).Document(order.Id.ToString());
//            await docRef.SetAsync(ToOrderDoc(order));

//            await UpdateClothRemainingMetersAsync(order.ClothId, order.MetersUsed);
//        }
//        catch 
//        {
//            return;
//        }
//    }

//    public async Task AddEmployeeAsync(Employee employee)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            employee.Id = await GetNextEmployeeIdAsync(db);
//            var docRef = db.Collection(EmployeesCollection).Document(employee.Id.ToString());
//            await docRef.SetAsync(ToEmployeeDoc(employee));
//        }
//        catch 
//        {
//            return;
//        }
//    }

//    public async Task UpdateClothAsync(Cloth cloth)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var docRef = db.Collection(ClothCollection).Document(cloth.Id.ToString());
//            await docRef.SetAsync(ToClothDoc(cloth), SetOptions.MergeAll);
//        }
//        catch
//        {
//            return;
//        }
//    }

//    public async Task UpdateOrderAsync(DressOrder order)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var docRef = db.Collection(OrdersCollection).Document(order.Id.ToString());
//            await docRef.SetAsync(ToOrderDoc(order), SetOptions.MergeAll);
//        }
//        catch
//        {
//            return;
//        }
//    }

//    public async Task DeleteClothAsync(int clothId)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var docRef = db.Collection(ClothCollection).Document(clothId.ToString());
//            await docRef.DeleteAsync();
//        }
//        catch
//        {
//            return;
//        }
//    }

//    public async Task DeleteOrderAsync(int orderId)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var docRef = db.Collection(OrdersCollection).Document(orderId.ToString());
//            await docRef.DeleteAsync();
//        }
//        catch
//        {
//            return;
//        }
//    }

//    public async Task UpdateOrderStatusAsync(int orderId, DressOrderStatus status)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var docRef = db.Collection(OrdersCollection).Document(orderId.ToString());
//            await docRef.UpdateAsync(new Dictionary<string, object>
//            {
//                [nameof(DressOrder.Status)] = status
//            });
//        }
//        catch
//        {
//            return;
//        }
//    }

//    public async Task UpdateClothRemainingMetersAsync(int clothId, double metersUsed)
//    {
//        try
//        {
//            var db = await GetDbAsync();
//            var docRef = db.Collection(ClothCollection).Document(clothId.ToString());
//            await db.RunTransactionAsync(async transaction =>
//                 {
//                     var snapshot = await transaction.GetSnapshotAsync(docRef);
//                     if (snapshot.Exists)
//                     {
//                         var currentRemaining = snapshot.GetValue<double>(nameof(Cloth.RemainingMeters));
//                         var newRemaining = Math.Max(0, currentRemaining - metersUsed);

//                         transaction.Update(docRef, new Dictionary<string, object>
//                         {
//                             [nameof(Cloth.RemainingMeters)] = newRemaining
//                         });
//                     }
//                 });
//        }
//        catch
//        {
//            return;
//        }
//    }

//    private static async Task<int> GetNextOrderIdAsync(FirestoreDb db, CancellationToken ct = default)
//    {
//        var countersRef = db.Document(CountersDoc);
//        int newId = 0;
//        await db.RunTransactionAsync(async transaction =>
//        {
//            var snap = await transaction.GetSnapshotAsync(countersRef);
//            int lastId = 0;
//            if (snap.Exists && snap.TryGetValue("lastOrderId", out int v)) lastId = v;
//            newId = lastId + 1;
//            transaction.Set(countersRef, new Dictionary<string, object> { ["lastOrderId"] = newId }, SetOptions.MergeAll);
//        }, cancellationToken: ct);
//        return newId;
//    }


//    private static async Task<int> GetNextClothIdAsync(FirestoreDb db, CancellationToken ct = default)
//    {
//        try
//        {
//            var countersRef = db.Document(CountersDoc);
//            int newId = 0;
//            await db.RunTransactionAsync(async transaction =>
//            {
//                var snap = await transaction.GetSnapshotAsync(countersRef);
//                int lastId = 0;
//                if (snap.Exists && snap.TryGetValue("lastClothId", out int v)) lastId = v;
//                newId = lastId + 1;
//                transaction.Set(countersRef, new Dictionary<string, object> { ["lastClothId"] = newId }, SetOptions.MergeAll);
//            }, cancellationToken: ct);
//            return newId;
//        }
//        catch 
//        {
//            return 1;
//        }
//    }

//    private static Task<int> GetNextEmployeeIdAsync(FirestoreDb db, CancellationToken ct = default)
//    {
//        var countersRef = db.Document(CountersDoc);
//        int newId = 0;
//        return db.RunTransactionAsync(async transaction =>
//        {
//            var snap = await transaction.GetSnapshotAsync(countersRef);
//            int lastId = 0;
//            if (snap.Exists && snap.TryGetValue("lastEmployeeId", out int v)) lastId = v;
//            newId = lastId + 1;
//            transaction.Set(countersRef, new Dictionary<string, object> { ["lastEmployeeId"] = newId }, SetOptions.MergeAll);
//            return newId;
//        }, cancellationToken: ct);
//    }


//private static IDictionary<string , object> ToClothDoc(Cloth c)
//    {
//        return new Dictionary<string, object>
//        {
//            [nameof(Cloth.Id)] = c.Id,
//            [nameof(Cloth.Name)] = c.Name,
//            [nameof(Cloth.UniqueCode)] = "CLT-"+ c.Id.ToString(),
//            [nameof(Cloth.Color)] = c.Color,
//            [nameof(Cloth.PricePerMeter)] = c.PricePerMeter,
//            [nameof(Cloth.TotalMeters)] = c.TotalMeters,
//            [nameof(Cloth.RemainingMeters)] = c.RemainingMeters,
//            [nameof(Cloth.AddedDate)] = c.AddedDate.ToUniversalTime(),
//        };
//    }

//    private static Cloth FromClothDoc(IDictionary<string, object> d)
//    {
//        var model = new Cloth
//        {
//            Id = d.TryGetValue(nameof(Cloth.Id), out var id) ? Convert.ToInt32(id) : 0,
//            Name = d.TryGetValue(nameof(Cloth.Name), out var name) ? name?.ToString() ?? string.Empty : string.Empty,
//            UniqueCode = d.TryGetValue(nameof(Cloth.UniqueCode), out var uniqueCode) ? uniqueCode?.ToString() ?? string.Empty : string.Empty,
//            Color = d.TryGetValue(nameof(Cloth.Color), out var color) ? color?.ToString() ?? string.Empty : string.Empty,
//            PricePerMeter = d.TryGetValue(nameof(Cloth.PricePerMeter), out var ppm) ? Convert.ToDouble(ppm) : 0,
//            TotalMeters = d.TryGetValue(nameof(Cloth.TotalMeters), out var tm) ? Convert.ToDouble(tm) : 0,
//            RemainingMeters = d.TryGetValue(nameof(Cloth.RemainingMeters), out var rm) ? Convert.ToDouble(rm) : 0,
//            AddedDate = d.TryGetValue(nameof(Cloth.AddedDate), out var ad) && ad is Timestamp ts ? ts.ToDateTime() : DateTime.MinValue,
//        };
//        return model;
//    }


//    private static Dictionary<string, object> ToOrderDoc(DressOrder o)
//    {
//        return new Dictionary<string, object>
//        {
//            [nameof(DressOrder.Id)] = o.Id,
//            [nameof(DressOrder.UniqueCode)] = "ORD-" + o.Id.ToString(),
//            [nameof(DressOrder.CustomerName)] = o.CustomerName,
//            [nameof(DressOrder.MobileNumber)] = o.MobileNumber,
//            [nameof(DressOrder.ClothId)] = o.ClothId,
//            [nameof(DressOrder.MetersUsed)] = o.MetersUsed,
//            [nameof(DressOrder.DressType)] = o.DressType,
//            [nameof(DressOrder.OrderDate)] = o.OrderDate.ToUniversalTime(),
//            [nameof(DressOrder.AssignedTo)] = o.AssignedTo,
//            [nameof(DressOrder.Status)] = o.Status.ToString(),
//        };
//    }
//    private static DressOrder FromOrderDoc(IDictionary<string, object> d)
//    {
//        var model = new DressOrder
//        {
//            Id = d.TryGetValue(nameof(DressOrder.Id), out var id) ? Convert.ToInt32(id) : 0,
//            UniqueCode = d.TryGetValue(nameof(DressOrder.UniqueCode), out var uniqueCode) ? uniqueCode?.ToString() ?? string.Empty : string.Empty,
//            CustomerName = d.TryGetValue(nameof(DressOrder.CustomerName), out var cn) ? cn?.ToString() ?? string.Empty : string.Empty,
//            MobileNumber = d.TryGetValue(nameof(DressOrder.MobileNumber), out var mn) ? mn?.ToString() ?? string.Empty : string.Empty,
//            ClothId = d.TryGetValue(nameof(DressOrder.ClothId), out var cid) ? Convert.ToInt32(cid) : 0,
//            MetersUsed = d.TryGetValue(nameof(DressOrder.MetersUsed), out var mu) ? Convert.ToDouble(mu) : 0,
//            DressType = d.TryGetValue(nameof(DressOrder.DressType), out var dt) ? dt?.ToString() ?? string.Empty : string.Empty,
//            OrderDate = d.TryGetValue(nameof(DressOrder.OrderDate), out var od) && od is Timestamp ts ? ts.ToDateTime() : DateTime.MinValue,
//            AssignedTo = d.TryGetValue(nameof(DressOrder.AssignedTo), out var at) ? (at != null ? Convert.ToInt32(at) : 0) : 0,
//            Status = d.TryGetValue(nameof(DressOrder.Status), out var st) && st != null && Enum.TryParse<DressOrderStatus>(st.ToString(), out var status) ? status : DressOrderStatus.Pending,
//        };

//        return model;
//    }


//    private static Dictionary<string, object> ToEmployeeDoc(Employee e)
//    {
//        return new Dictionary<string, object>
//        {
//            [nameof(Employee.Id)] = e.Id,
//            [nameof(Employee.Name)] = e.Name,
//            [nameof(Employee.Username)] = e.Username,
//            [nameof(Employee.Password)] = e.Password,
//            [nameof(Employee.MobileNumber)] = e.MobileNumber,
//        };
//    }

//    private static Employee FromEmployee(IDictionary<string, object> d)
//    {
//        var model = new Employee
//        {
//            Id = d.TryGetValue(nameof(Employee.Id), out var id) ? Convert.ToInt32(id) : 0,
//            Name = d.TryGetValue(nameof(Employee.Name), out var name) ? name?.ToString() ?? string.Empty : string.Empty,
//            Username = d.TryGetValue(nameof(Employee.Username), out var username) ? username?.ToString() ?? string.Empty : string.Empty,
//            Password = d.TryGetValue(nameof(Employee.Password), out var password) ? password?.ToString() ?? string.Empty : string.Empty,
//            MobileNumber = d.TryGetValue(nameof(Employee.MobileNumber), out var mobile) ? mobile?.ToString() ?? string.Empty : string.Empty,
//        };
//        return model;
//    }

//}