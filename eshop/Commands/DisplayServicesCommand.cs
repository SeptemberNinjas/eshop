using System.Text;
using eshop.Core;

namespace eshop.Commands;

/// <summary>
/// Команда отображения списка услуг
/// </summary>
public class DisplayServicesCommand
{
    private readonly Service[] _services;
    
    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "DisplayServices";
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Вывести список услуг";
    }

    /// <inheritdoc cref="DisplayServicesCommand"/>
    public DisplayServicesCommand(Service[] services)
    {
        _services = services;
    }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    public string Execute(string[]? args)
    {
        if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
        {
            count = _services.Length;
        }
 
        var message = new StringBuilder();
        for (var i = 0; i < Math.Min(_services.Length, count); i++)
        {
            message.AppendLine(_services[i].GetDisplayText());
        }

        return message.ToString();
    }
}