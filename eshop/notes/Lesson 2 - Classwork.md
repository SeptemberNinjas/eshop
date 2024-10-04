**План занятия:**
- Реализовать каркас консольного приложения для будущей реализации интернет-магазина.
- Реализовать служебные команды (отобразить список команд и выход из приложения) для демонстрации процесса добавления команд.
- Реализовать сборку Core в которой будет находиться сущность "Товар" (сущность тоже реализовать).

Пишем код:
1. Переделываем Program в классический вид (если нужно).
```csharp
namespace eshop;

public static class Program
{
    public static void Main(string[] args)
    {
    }    
}
```
2. Реализуем цикл запроса команд в методе Main
```csharp
public static void Main(string[] args)
{
    Console.WriteLine("Программа: 'Интернет магазин'");  
        
    // Механизм выхода реализуем дальше, пока оставляем так
    while (true)
    {
        Console.WriteLine("Выполните команду");
        var command = Console.ReadLine();
        Execute(command);
    }
}
```
3. Реализуем метод Execute
```csharp
private static void Execute(string command)
{
    // Показываем базовую валидацию ввода пользователя +
    if (string.IsNullOrEmpty(command))
    {
        Console.WriteLine("Ошибка: неизвестная команда");
        return;
    }

    // Split им должнен быть непонятен, объяснить как работает
    var commandNameWithArgs = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    
    // Объяснить формирование массива аргументов
    var commandName = commandNameWithArgs[0];
    var args = new string[commandNameWithArgs.Length - 1];
    for (var i = 0; i < args.Length; i++)
    {
        args[i] = commandNameWithArgs[i + 1];
    }

    switch (commandName)
    {
        // Команду добавим дальше
        default:
            Console.WriteLine("Ошибка: неизвестная команда");
            break;
    }
}
```
4. Реализуем команду отображения списка команд
```csharp
namespace eshop.Commands;

// Потом можно выделить общий класс команды
public static class DisplayCommandsCommand
{
    public const string Name = "DisplayCommands";
    
    public static string GetInfo()
    {
        return "Вывести список команд";
    }
    
    public static void Execute()
    {
        // Повод показать перегрузку ToString (только нужно от статиков уйти)
        Console.WriteLine($"{DisplayCommandsCommand.Name} - {DisplayCommandsCommand.GetInfo()}");
    }
}
```
5. Добавляем команду в движок
```csharp
public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Программа: 'Интернет магазин'");
        // Добавляем вызов при старте приложения
        DisplayCommandsCommand.Execute();        
        ...
    }

    private static void Execute(string command)
    {
        ...
        switch (commandName)
        {
            // Показать, что если Name сделать полем, а не константой, то так не получится
            case DisplayCommandsCommand.Name:
                // Потом (когда дойдем до делегатов) вызов можно реализовать 
                // через словарь Dictionary<string, Action<string[]>> 
                DisplayCommandsCommand.Execute();
                break;
            default:
                Console.WriteLine("Ошибка: неизвестная команда");
                break;
        }
    }
}
```
6. Реализуем и добавляем команду выхода
```csharp
// ExitCommand.cs
namespace eshop.Commands;

public static class ExitCommand
{
    public const string Name = "Exit";
    
    public static string GetInfo()
    {
        return "Выйти из программы";
    }
    
    public static void Execute()
    {       
        // Выходим без костылей :)
        Environment.Exit(0);
    }
}
...
// DisplayCommandsCommand.cs
public static void Execute()
{
    // Когда замутят базовый класс команды можно будет по красоте сделать
    Console.WriteLine($"{DisplayCommandsCommand.Name} - {DisplayCommandsCommand.GetInfo()}");
    Console.WriteLine($"{ExitCommand.Name} - {ExitCommand.GetInfo()}");
}
...
// Program.cs
private static void Execute(string command)
{
    ...
    switch (commandName)
    {
        ...
        case ExitCommand.Name:            
            ExitCommand.Execute();
            break;
        ...
    }
}
```
7. Добавляем проект Core и сущность товара
```csharp
namespace eshop.Core;

// Прививаем код стайл

/// <summary>
/// Товар
/// </summary>
public class Product
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id; // Потом сделают свойствами

    /// <summary>
    /// Наименование
    /// </summary>
    public string Name;

    /// <summary>
    /// Цена
    /// </summary>
    public decimal Price;

    /// <summary>
    /// Остатки
    /// </summary>
    public int Remains;

    /// <inheritdoc cref="Product"/>
    public Product(int id, string name, decimal price, int remains)
    {
        Id = id;
        Name = name;
        Price = price;
        Remains = remains;
    }
    
    // Метод отображения пусть добавят в домашке
}
```