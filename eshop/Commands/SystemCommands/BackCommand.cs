namespace eshop.Commands.SystemCommands;

/// <summary>
/// Команда возврата к предыдущей странице
/// </summary>
public class BackCommand : IEshopCommand
{
    public const string Info = "Вернуться к предыдущей странице";

    /// <inheritdoc />
    public override string ToString() => Info;

    /// <inheritdoc />
    public string? Result => string.Empty;

    /// <inheritdoc />
    public void Execute(string[]? args)
    {
        // Специальная команда 
    }
}