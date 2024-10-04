using eshop.Core;

namespace eshop.Commands;

/// <summary>
/// Команда отображения списка услуг
/// </summary>
public class DisplayServicesCommand
{
    private readonly Service[] _services;
    
    public const string Name = "DisplayServices";
    
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
    public void Execute(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
        {
            count = _services.Length;
        }
 
        for (var i = 0; i < Math.Min(_services.Length, count); i++)
        {
            Console.WriteLine(_services[0].GetDisplayText());
        }
    }
}