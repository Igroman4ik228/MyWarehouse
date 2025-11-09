using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWarehouse.Models.Entities;

public class Stock
{
    [Key]
    public int IdStocks { get; set; }

    public int ProductId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
    public int ProductQuantity { get; set; }

    public int LocationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Максимальное количество должно быть больше 0")]
    public int MaxProductQuantity { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [ForeignKey("LocationId")]
    public Location Location { get; set; }
}

public class Product
{
    [Key]
    public int IdProduct { get; set; }

    [StringLength(200, ErrorMessage = "Название не может превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Описание не может превышать 1000 символов")]
    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public int UnitId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Вес не может быть отрицательным")]
    public decimal Weight { get; set; }

    [StringLength(50, ErrorMessage = "SKU не может превышать 50 символов")]
    public string SKU { get; set; } = string.Empty;

    public DateTime? ExpirationDate { get; set; }

    public bool IsFragile { get; set; }

    public bool IsWaterSensitive { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Длина не может быть отрицательной")]
    public decimal Length { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Ширина не может быть отрицательной")]
    public decimal Width { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Высота не может быть отрицательной")]
    public decimal Height { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; }

    [ForeignKey("UnitId")]
    public Unit Unit { get; set; }

    public ICollection<Stock> Stocks { get; set; } = [];
    public ICollection<DeliveryTask> DeliveryTasks { get; set; } = [];
}

public class Category
{
    [Key]
    public int IdCategory { get; set; }


    [StringLength(100, ErrorMessage = "Название категории не может превышать 100 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Описание категории не может превышать 500 символов")]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}

public class Unit
{
    [Key]
    public int IdUnit { get; set; }


    [StringLength(50, ErrorMessage = "Название единицы измерения не может превышать 50 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Описание единицы измерения не может превышать 200 символов")]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}

public class Location
{
    [Key]
    public int IdLocation { get; set; }


    [StringLength(50, ErrorMessage = "Полка не может превышать 50 символов")]
    public string Shelf { get; set; } = string.Empty;


    [StringLength(50, ErrorMessage = "Ячейка не может превышать 50 символов")]
    public string Cell { get; set; } = string.Empty;


    [StringLength(50, ErrorMessage = "Зона не может превышать 50 символов")]
    public string Zone { get; set; } = string.Empty;


    [Range(0, double.MaxValue, ErrorMessage = "Максимальная длина не может быть отрицательной")]
    public decimal MaxLength { get; set; }


    [Range(0, double.MaxValue, ErrorMessage = "Максимальная ширина не может быть отрицательной")]
    public decimal MaxWidth { get; set; }


    [Range(0, double.MaxValue, ErrorMessage = "Максимальная высота не может быть отрицательной")]
    public decimal MaxHeight { get; set; }


    [Range(0, double.MaxValue, ErrorMessage = "Максимальный вес не может быть отрицательным")]
    public decimal MaxWeight { get; set; }

    public ICollection<Stock> Stocks { get; set; } = [];
    public ICollection<DeliveryTask> FromDeliveryTasks { get; set; } = [];
    public ICollection<DeliveryTask> ToDeliveryTasks { get; set; } = [];

    [NotMapped]
    public string DisplayName => $"{Zone} — Полка: {Shelf}, Ячейка: {Cell}";
}

public class Client
{
    [Key]
    public int IdClient { get; set; }


    [StringLength(200, ErrorMessage = "Имя клиента не может превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Описание клиента не может превышать 500 символов")]
    public string? Description { get; set; }


    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [StringLength(100, ErrorMessage = "Email не может превышать 100 символов")]
    public string Email { get; set; } = string.Empty;


    [Phone(ErrorMessage = "Некорректный формат телефона")]
    [StringLength(20, ErrorMessage = "Телефон не может превышать 20 символов")]
    public string Phone { get; set; } = string.Empty;

    public ICollection<DeliveryTask> DeliveryTasks { get; set; } = [];
}

public class DeliveryTask
{
    [Key]
    public int IdDeliveryTask { get; set; }


    public int ProductId { get; set; }


    public int ClientId { get; set; }


    [Range(1, int.MaxValue, ErrorMessage = "Количество товара должно быть больше 0")]
    public int ProductQuantity { get; set; }


    public DateTime CreatedAt { get; set; }


    public int CreatedUserId { get; set; }


    public int TaskStatusId { get; set; }


    public int DeliveryTypeId { get; set; }

    public int? ExecutorUserId { get; set; }

    public int? FromLocationId { get; set; }

    public int? ToLocationId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [ForeignKey("ClientId")]
    public Client Client { get; set; }

    [ForeignKey("CreatedUserId")]
    public User CreatedUser { get; set; }

    [ForeignKey("ExecutorUserId")]
    public User? ExecutorUser { get; set; }

    [ForeignKey("TaskStatusId")]
    public TaskStatus TaskStatus { get; set; }

    [ForeignKey("DeliveryTypeId")]
    public DeliveryType DeliveryType { get; set; }

    [ForeignKey("FromLocationId")]
    public Location? FromLocation { get; set; }

    [ForeignKey("ToLocationId")]
    public Location? ToLocation { get; set; }
}

public class User
{
    [Key]
    public int IdUser { get; set; }

    [StringLength(50, ErrorMessage = "Имя не может превышать 50 символов")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Логин не может превышать 50 символов")]
    public string Login { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Пароль не может превышать 255 символов")]
    public string Password { get; set; } = string.Empty;

    public int RoleId { get; set; }

    [StringLength(50, ErrorMessage = "Отчество не может превышать 50 символов")]
    public string? Patronymic { get; set; }

    [ForeignKey("RoleId")]
    public Role Role { get; set; }

    public ICollection<DeliveryTask> CreatedTasks { get; set; } = [];
    public ICollection<DeliveryTask> ExecutedTasks { get; set; } = [];
}

public class Role
{
    [Key]
    public int IdRole { get; set; }

    [StringLength(50, ErrorMessage = "Название роли не может превышать 50 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Описание роли не может превышать 200 символов")]
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = [];
}

public class DeliveryType
{
    [Key]
    public int IdDeliveryType { get; set; }

    [StringLength(50, ErrorMessage = "Название типа доставки не может превышать 50 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Описание типа доставки не может превышать 200 символов")]
    public string? Description { get; set; }

    public ICollection<DeliveryTask> DeliveryTasks { get; set; } = [];
}

public class TaskStatus
{
    [Key]
    public int IdTaskStatus { get; set; }

    [StringLength(50, ErrorMessage = "Название статуса не может превышать 50 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Описание статуса не может превышать 200 символов")]
    public string? Description { get; set; }

    public ICollection<DeliveryTask> DeliveryTasks { get; set; } = [];
}