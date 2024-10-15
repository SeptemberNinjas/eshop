using System.Text;
using eshop.Commands;
using eshop.Commands.SystemCommands;

namespace eshop;

public class ConsolePage
{
    private readonly ApplicationContext _context;
    private readonly ConsolePage? _prev;
    
    private ICommandWithCommandsList? _initialCommand;
    private string[]? _args;
    private bool _lastCommandIsGoToRoot;

    public ConsolePage(ApplicationContext context, ICommandWithCommandsList initialCommand, string[]? args,
        ConsolePage? prev = null)
    {
        _context = context;
        _initialCommand = initialCommand;
        _args = args;
        _prev = prev;
    }

    public void WaitForInput()
    {
        while (true)
        {
            (IEshopCommand command, string[]? args) nextCommand = default;
            while (nextCommand == default)
            {
                Console.WriteLine("Выполните команду");
                var command = Console.ReadLine();
                nextCommand = GetNextCommand(command, _initialCommand as ICommandWithContext);
            }

            if (nextCommand.command is GoToRootPageCommand && _prev is not null)
            {
                _lastCommandIsGoToRoot = true;
                _prev._lastCommandIsGoToRoot = true;
                break;
            }

            if (nextCommand.command is BackCommand && _prev is not null)
                break;

            if (nextCommand.command is ICommandWithCommandsList commandWithContext)
            {
                if (nextCommand.command.GetType() != _initialCommand?.GetType())
                {
                    commandWithContext.Execute(nextCommand.args);
                    if (!commandWithContext.ExecutionSuccess)
                    {
                        DisplayInitial();
                        Console.WriteLine(commandWithContext.Result);
                        continue;
                    }
                    var nextPage = new ConsolePage(_context, commandWithContext, nextCommand.args, this);
                    nextPage.DisplayInitial();
                    nextPage.WaitForInput();
                    DisplayInitial();
                    if (_prev is null || nextPage._lastCommandIsGoToRoot)
                        break;
         
                    continue;
                }

                _initialCommand = commandWithContext;
                _args = nextCommand.args;
                _initialCommand.Execute(_args);
                DisplayInitial();
            }
            else
            {
                DisplayInitial();
                nextCommand.command?.Execute(nextCommand.args);
                Console.WriteLine(nextCommand.command?.Result);
            }
        }
    }

    public void DisplayInitial()
    {
        Console.Clear();
        if (_initialCommand is not null)
            Console.WriteLine(_initialCommand.Result);
        DisplayAvailableCommands();
    }

    private void DisplayAvailableCommands()
    {
        if (_initialCommand is null || _initialCommand.AvailableCommands.Count == 0)
            return;

        var commandsList = new StringBuilder();
        var descriptions = _initialCommand.AvailableCommands.Values.ToArray();
        var maxLength = 0;
        for (var i = 0; i < descriptions.Length; i++)
        {
            var line = $"  {i + 1} - {descriptions[i]}";
            maxLength = Math.Max(maxLength, line.Length);
            commandsList.AppendLine(line);
        }

        var dashes = string.Empty.PadRight(maxLength + 2, '-');
        var message = new StringBuilder(dashes)
            .AppendLine()
            .Append(commandsList)
            .AppendLine(dashes)
            .AppendLine();

        Console.WriteLine(message);
    }

    private (IEshopCommand, string[]?) GetNextCommand(string? input, ICommandWithContext? lastCommand = null)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Ошибка: неизвестная команда");
            return default;
        }

        var commandNameWithArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var commandName = commandNameWithArgs[0];
        var args = new string[commandNameWithArgs.Length - 1];
        for (var i = 0; i < args.Length; i++)
        {
            args[i] = commandNameWithArgs[i + 1];
        }

        if (!int.TryParse(commandName, out var commandNumber) ||
            commandNumber > _initialCommand?.AvailableCommands.Count)
        {
            Console.WriteLine("Ошибка: неизвестная команда");
            return default;
        }

        var commandType = _initialCommand?.AvailableCommands.Keys.ToArray()[commandNumber - 1];
        if (commandType is null)
            return default;

        
        var command = _context.CreateCommand(commandType.Value);
        if (command is ICommandWithContext contextCommand)
            contextCommand.Context = lastCommand?.Context;

        return (command, args);
    }
}