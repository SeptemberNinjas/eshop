namespace eshop.Commands;

public interface ICommandWithCommandsList : IEshopCommand
{
    public bool ExecutionSuccess { get; }
    IReadOnlyDictionary<CommandType, string> AvailableCommands { get; }
}