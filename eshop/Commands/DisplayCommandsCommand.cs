namespace eshop.Commands;

/// <summary>
/// Команда отображения команд
/// </summary>
public static class DisplayCommandsCommand
{
    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "DisplayCommands";
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Вывести список команд";
    }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    public static string Execute()
    {
        var messages = new[]
        {
            $"{DisplayCommandsCommand.Name} - {DisplayCommandsCommand.GetInfo()}",
            $"{ExitCommand.Name} - {ExitCommand.GetInfo()}",
            $"{DisplayProductsCommand.Name} - {DisplayProductsCommand.GetInfo()}",
            $"{DisplayServicesCommand.Name} - {DisplayServicesCommand.GetInfo()}",
            $"{AddBasketLineCommand.Name} - {AddBasketLineCommand.GetInfo()}"
        };

        return string.Join('\n', messages);
    }
}