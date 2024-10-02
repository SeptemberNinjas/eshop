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
    }
}