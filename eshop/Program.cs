namespace eshop;

public static class Program
{
    private static readonly ApplicationContext App = new ();
    
    public static void Main(string[] args)
    {
        Console.WriteLine(ApplicationContext.Title);
        var output = App.ExecuteStartupCommand();
        Console.WriteLine(output);

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

        var output = App.ExecuteCommandByName(commandName, args);
        Console.WriteLine(output);
    }
}