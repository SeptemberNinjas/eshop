namespace eshop.Commands;

/// <summary>
/// Команда выхода из приложения
/// </summary>
public static class ExitCommand
{
    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "Exit";
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Выйти из программы";
    }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    public static string Execute()
    {       
        Environment.Exit(0);
        
        return string.Empty;
    }
}