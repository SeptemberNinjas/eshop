namespace eshop.Commands.SystemCommands;

/// <summary>
/// Команда выхода из приложения
/// </summary>
public class ExitCommand : IEshopCommand
{
    public const string Info = "Выйти из программы";

    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result => string.Empty;

    /// <inheritdoc />
    public Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}