namespace eshop.Commands.SystemCommands;

/// <summary>
/// Команда возврата к стартовой странице
/// </summary>
public class GoToRootPageCommand : IEshopCommand
{
    public const string Info = "Вернуться к стартовой странице";

    /// <inheritdoc />
    public override string ToString() => Info;

    public string? Result => string.Empty;
    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        // Специальная команда
    }
}