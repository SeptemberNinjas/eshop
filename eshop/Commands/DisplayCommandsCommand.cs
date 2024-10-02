namespace eshop.Commands;

public static class DisplayCommandsCommand
{
    public const string Name = "DisplayCommands";
    
    public static string GetInfo()
    {
        return "Вывести список команд";
    }
    
    public static void Execute()
    {
        Console.WriteLine($"{DisplayCommandsCommand.Name} - {DisplayCommandsCommand.GetInfo()}");
        Console.WriteLine($"{ExitCommand.Name} - {ExitCommand.GetInfo()}");
        Console.WriteLine($"{DisplayProductsCommand.Name} - {DisplayProductsCommand.GetInfo()}");
        Console.WriteLine($"{DisplayServicesCommand.Name} - {DisplayServicesCommand.GetInfo()}");
    }
}