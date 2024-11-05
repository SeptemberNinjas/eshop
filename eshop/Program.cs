using Microsoft.Extensions.Configuration;

namespace eshop;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var confBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var app = new ApplicationContext(confBuilder);

        Console.WriteLine(ApplicationContext.Title);
        var initialCommand = app.GetInitialCommand();
        await initialCommand.ExecuteAsync(null, CancellationToken.None);
        var page = new ConsolePage(app, initialCommand, null);
        
        while (true)
        {
            page.DisplayInitial();
            await page.WaitForInput(CancellationToken.None);
        }
    }
}