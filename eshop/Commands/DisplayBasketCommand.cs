using eshop.Core;

namespace eshop.Commands;

/// <summary>
/// Команда отображения корзины
/// </summary>
public class DisplayBasketCommand
{
    private readonly Basket _basket;

    /// <summary>
    /// Имя команды
    /// </summary>
    public const string Name = "DisplayBasket";
    
    /// <summary>
    /// Получить описание команды
    /// </summary>
    public static string GetInfo()
    {
        return "Отобразить корзину";
    }
    
    /// <inheritdoc cref="DisplayBasketCommand"/>
    public DisplayBasketCommand(Basket basket)
    {
        _basket = basket;
    }

    /// <summary>
    /// Выполнить команду
    /// </summary>
    public string Execute()
    {
        return _basket.ToString();
    }
}