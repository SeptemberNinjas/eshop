using eshop.Commands;

namespace eshop;

public static class Program
{
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
            default:
                Console.WriteLine("Ошибка: неизвестная команда");
                break;
        }
    }
}