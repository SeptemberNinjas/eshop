using eshop.Commands;
using eshop.Core;

namespace eshop;

public static class Program
{
    private static readonly Product[] Products = new[]
    {
        new Product(1, "Лопата", 9.99m, 3),
        new Product(2, "Трактор", 300, 4)
    };
    
    private static readonly Service[] Services = new[]
    {
        new Service(3, "Раскопать яму", 5.49m),
        new Service(4, "Вспахать поле", 1000)
    };
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Программа: 'Интернет магазин'");
        DisplayCommandsCommand.Execute();

        while (true)
        {
            Console.WriteLine("Выполните команду");
            var command = Console.ReadLine();
            Execute(command);
        }
    }

    private static void Execute(string command)
    {
        if (string.IsNullOrEmpty(command))
        {
            Console.WriteLine("Ошибка: неизвестная команда");
            return;
        }
        
        var commandNameWithArgs = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var commandName = commandNameWithArgs[0];
        var args = new string[commandNameWithArgs.Length - 1];
        for (var i = 0; i < args.Length; i++)
        {
            args[i] = commandNameWithArgs[i + 1];
        }

        switch (commandName)
        {
            case DisplayCommandsCommand.Name:
                DisplayCommandsCommand.Execute();
                break;
            case ExitCommand.Name:
                ExitCommand.Execute();
                break;
            case DisplayProductsCommand.Name:
                var productsCommand = new DisplayProductsCommand(Products);
                productsCommand.Execute(args);
                break;
            case DisplayServicesCommand.Name:
                var servicesCommand = new DisplayServicesCommand(Services);
                servicesCommand.Execute(args);
                break;
            default:
                Console.WriteLine("Ошибка: неизвестная команда");
                break;
        }
    }
}