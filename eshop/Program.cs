using eshop.Commands;

namespace eshop;

public static class Program
{
    private static readonly ApplicationContext App = new ();
    public static void Main(string[] args)
    {
        Console.WriteLine(ApplicationContext.Title);
        var initialCommand = App.GetInitialCommand();
        initialCommand.Execute(null);
        var page = new ConsolePage(App, initialCommand, null);
        page.DisplayInitial();
        while (true)
        {
            page.WaitForInput();
        }
    }
}