namespace eshop.Commands;

public interface IEshopCommand
{
    public string? Result { get; }
    
    /// <summary>
    /// Выполнить команду
    /// </summary>
    Task ExecuteAsync(string[]? args, CancellationToken cancellationToken);
}