using Microsoft.Extensions.Configuration;

namespace eshop;

public static class Program
{
    public static void Main(string[] args)
    {
        var confBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var app = new ApplicationContext(confBuilder);

        Console.WriteLine(ApplicationContext.Title);
        var initialCommand = app.GetInitialCommand();
        initialCommand.Execute(null);
        var page = new ConsolePage(app, initialCommand, null);
        
        while (true)
        {
            page.DisplayInitial();
            page.WaitForInput();
        }
    }
}