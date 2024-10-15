namespace eshop.Commands;

public interface ICommandWithContext
{
    object? Context { get; set; }
}