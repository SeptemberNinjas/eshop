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
    public void Execute(string[]? args)
    {
        Environment.Exit(0);
    }
}