namespace eshop.Commands;

public interface IEshopCommand
{
    public string? Result { get; }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    void Execute(string[]? args);
}