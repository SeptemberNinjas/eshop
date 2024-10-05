**План занятия:**
- В консольном приложении выделить суперкласс Application, внутри которого будут находиться реализации команд, хранение сущностей и все прочее. Исключить зависимость от консоли.
- Добавить корзину и процесс добавления товара в корзину.

Пишем код:
1. Рефакторим классы товара с использованием свойств. Тут нужно показать преимущества свойств перед полями.
```csharp
...
    private int _stock;

    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Наименование
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Цена
    /// </summary>
    public decimal Price { get; }
    
    /// <summary>
    /// Остатки
    /// </summary>
    public int Stock
    {
        get 
        {
            return _stock;
        }
        set 
        {
            if (value < 0)                
                _stock = 0;
            else
                _stock = value;
        }
    }
    // Для остатков реализуем свойства с явными гетерами и сеттерами, т.к.
    // в коде они будут встречать синтаксис на стрелках, то думаю надо сразу им показать
    // такой вариант (за одно показать тернарник): 
    public int Stock
    {
        get => _stock;
        set => _stock = value < 0 ? 0 : value;
    }
...
    
    // Тут же можно дать современного сахорочка с надёжно инициализируемым объектом, 
    // но без неудобных жирных конструкторов
    
    /// <summary>
    /// Цена
    /// </summary>
    public required decimal Price { get; init; }
    ...
    var product = new Product 
    {
        ...
        Price = price;
        ...
    {
```
2. Реализуем класс контекста приложения
```csharp
// Основная идея провести четкую границу между консолью и нашим приложением, 
// для этого следует все типы существующие в контексте приложения не должны зависить от консоли
public class ApplicationContext
{
    /// <summary>
    /// Описание приложения
    /// </summary>
    public const string Title = "Программа: 'Интернет магазин'"; // Забираем заголовок

    /// <summary>
    /// Список товаров
    /// </summary>
    private readonly Product[] _products = new[] // Забираем массивы захардкоженные в прошлом ДЗ
    {
        new Product(1, "Лопата", 9.99m, 3),
        new Product(2, "Трактор", 300, 4)
    };

    /// <summary>
    /// Список услуг
    /// </summary>
    private readonly Service[] _services = new[] // Забираем массивы захардкоженные в прошлом ДЗ
    {
        new Service(3, "Раскопать яму", 5.49m),
        new Service(4, "Вспахать поле", 1000)
    };


    /// <summary>
    /// Выполнить стартовую команду
    /// </summary>
    public string ExecuteStartupCommand() // Забираем детали выполнения стартовой команды
    {
        return ExecuteCommandByName(DisplayCommandsCommand.Name);
    }

    // Забираем в контекст детали выполнения команд
    
    /// <summary>
    /// Выполнить команду по имени
    /// </summary>
    public string ExecuteCommandByName(string commandName, string[]? args = null)
    {
        // Тут предполагается, что команды у нас будут реализовывать Func<string>
        string output;
        switch (commandName)
        {
            case DisplayCommandsCommand.Name:
                output = DisplayCommandsCommand.Execute();
                break;
            case ExitCommand.Name:
                output = ExitCommand.Execute();
                break;
            case DisplayProductsCommand.Name:
                output = new DisplayProductsCommand(_products).Execute(args);
                break;
            case DisplayServicesCommand.Name:
                output = new DisplayServicesCommand(_services).Execute(args);
                break;
            default:
                output = "Ошибка: неизвестная команда";
                break;
        }

        return output;
        ...
        // плюс предлагаю показать switch expression
        return commandName switch
        {
            DisplayCommandsCommand.Name => DisplayCommandsCommand.Execute(),
            ExitCommand.Name => ExitCommand.Execute(),
            DisplayProductsCommand.Name => new DisplayProductsCommand(_products).Execute(args),
            DisplayServicesCommand.Name => new DisplayServicesCommand(_services).Execute(args),
            var _ => "Ошибка: неизвестная команда"
        };
    }
}
```
3. Исправляем существующие команды
```csharp
// DisplayCommandsCommand.cs
public static string Execute()
{
    var messages = new[]
    {
        $"{DisplayCommandsCommand.Name} - {DisplayCommandsCommand.GetInfo()}",
        $"{ExitCommand.Name} - {ExitCommand.GetInfo()}",
        $"{DisplayProductsCommand.Name} - {DisplayProductsCommand.GetInfo()}",
        $"{DisplayServicesCommand.Name} - {DisplayServicesCommand.GetInfo()}"            
    };

    return string.Join('\n', messages);
}
...
// DisplayProductsCommand.cs
public string Execute(string[]? args)
{
    if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
    {
        count = _products.Length;
    }

    var message = new StringBuilder(); // Показать StringBuilder, расказать зачем он нужен
    for (var i = 0; i < Math.Min(_products.Length, count); i++)
    {
        message.AppendLine(_products[i].GetDisplayText());
    }

    return message.ToString();
}

// С сервисами и командой выхода тоже самое 
```
4. Реализуем корзину
```csharp
// Basket.cs

/// <summary>
/// Корзина
/// </summary>
public class Basket
{
    private readonly List<BasketLine> _lines = new ();

    /// <summary>
    /// Добавить товар в корзину
    /// </summary>
    public string AddLine(Product product, int requestedCount)
    {
        if (requestedCount < 1)
            return "Запрашиваемое количество товара должно быть больше 0";
        
        if (product.Stock < requestedCount)
            return $"Нельзя добавить товар в корзину, недостаточно остатков. Имеется {product.Stock}, Требуется {requestedCount}";

        product.Stock -= requestedCount;
        if (TryGetBasketLine(product, out var line))
            line.Count += requestedCount;
        else
            _lines.Add(new BasketLine(product, requestedCount));

        return $"В корзину добавлено {requestedCount} единиц товара \'{product.Name}\'";
    }
    
    /// <summary>
    /// Добавить услугу в корзину
    /// </summary>
    public string AddLine(Service service)
    {
        if (TryGetBasketLine(service, out var line))
            return $"Ошибка при добавлении услуги. Услуга \'{service.Name}\' уже добавлена в корзину";
        
        _lines.Add(new BasketLine(service));
        return $"В корзину добавлена услуга \'{service.Name}\'";
    }

    // Пример приватного метода
    private bool TryGetBasketLine(Product product, out BasketLine line)
    {
        foreach (var ln in _lines)
        {
            if (ln.ItemType != ItemTypes.Product || ln.ItemId != product.Id) 
                continue;
            line = ln;
            return true;
        }

        line = null!;
        return false;
    }
    
    // Пример перегрузки метода
    private bool TryGetBasketLine(Service service, out BasketLine line)
    {
        foreach (var ln in _lines)
        {
            if (ln.ItemType != ItemTypes.Service || ln.ItemId != service.Id) 
                continue;
            line = ln;
            return true;
        }

        line = null!;
        return false;
    }
}
...
// BasketLine.cs

/// <summary>
/// Линия корзины
/// </summary>
public class BasketLine
{
    private readonly Product? _product; // Не уверен, что надо nullable сюда
    private readonly Service? _service;
    private int _count;

    /// <summary>
    /// Идентификатор элемента
    /// </summary>
    public int ItemId => _product?.Id ?? _service!.Id; // Повод использовать ??

    /// <summary>
    /// Тип элемента
    /// </summary> // Ещё раз показываем тернарник
    public ItemTypes ItemType => _product is not null ? ItemTypes.Product : ItemTypes.Service;

    /// <summary>
    /// Текст, отображаемый в списке элементов корзины
    /// </summary>
    public string Text => $"{_product?.Name ?? _service!.Name} | {Count}";

    /// <summary>
    /// Количество элементов в корзине
    /// </summary>
    public int Count // Ещё пример со свойством с явным сеттером
    {
        get => _count;
        set
        {
            if (_service is not null || value < 1)
                return;

            _count = value;
        }
    }

    /// <inheritdoc cref="BasketLine"/>
    public BasketLine(Product product, int requestedCount)
    {
        _product = product;
        Count = requestedCount;
    }
    
    // Пример как линия работает по разному в зависимости от выбранного конструктора
    
    /// <inheritdoc cref="BasketLine"/>
    public BasketLine(Service service)
    {
        _service = service;
        _count = 1;
    }
}    
...
// ItemTypes.cs
/// <summary>
/// Типы элементов в корзине
/// </summary>
public enum ItemTypes
{
    /// <summary>
    /// Товар
    /// </summary>
    Product,
    
    /// <summary>
    /// Услуга
    /// </summary>
    Service
}
```
5. Добавляем команду добавления в корзину
```csharp
// AddBasketLineCommand.cs

/// <summary>
/// Команда добавления элемента в корзину
/// </summary>
public class AddBasketLineCommand
{
    private readonly Basket _basket;
    private readonly Product[] _products;
    private readonly Service[] _services;
    
    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "AddBasketLine";
    
    /// <inheritdoc cref="AddBasketLineCommand"/>
    public AddBasketLineCommand(Basket basket, Product[] products, Service[] services)
    {
        _basket = basket;
        _products = products;
        _services = services;
    }
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Добавить товар в корзину";
    }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    public string Execute(string[]? args)
    {
        if (args is null 
            || args.Length < 2 
            || !int.TryParse(args[0], out var id) 
            || !Enum.TryParse<ItemTypes>(args[1], out var type))
            return "Для добавления в корзину необходимо указать идентификатор, тип (товар или услуга) и количество (для товара)\n" +
                   $"Пример: {Name} 1 {ItemTypes.Product.ToString()} 3";

        var count = 0;
        if (type is ItemTypes.Product && (args.Length < 3 || !int.TryParse(args[2], out count)))
            return "Для добавления в корзину необходимо указать идентификатор, тип (товар или услуга) и количество (для товара)\n" +
                   $"Пример: {Name} 1 {ItemTypes.Product.ToString()} 3"; 
        // Тут копипаста сообщения об ошибке. Можно вынести в метод

        // Тут можно показать, что логика "Добавить если не существует" не должна находиться в команде
        // и её необходимо вынести в контекст
        switch (type)
        {
            case ItemTypes.Product:
                if (!TryGetProduct(id, out var product))
                    return $"Не найден товар с идентификатором {id}";
                return _basket.AddLine(product, count);
            case ItemTypes.Service:
                if (!TryGetService(id, out var service))
                    return $"Не найдена услуга с идентификатором {id}";
                return _basket.AddLine(service);
            default:
                return string.Empty;
        }
    }

    private bool TryGetService(int id, out Service service)
    {
        foreach (var s in _services)
        {
            if (s.Id != id)
                continue;
            service = s;
            return true;
        }

        service = null!;
        return false;
    }

    private bool TryGetProduct(int id, out Product product)
    {
        foreach (var pr in _products)
        {
            if (pr.Id != id)
                continue;
            product = pr;
            return true;
        }

        product = null!;
        return false;
    }
}

// также добавить в списки вызова и отображения команд 
```
