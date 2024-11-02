using Microsoft.Extensions.Configuration;

namespace eshop;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"""
                Произошила ошибка в работе приложения: {ex.Message}
                { ex.StackTrace}
                """);
            Console.WriteLine("Нажмите Enter чтобы закрыть приложение ...");
            Console.ReadLine();
        }
    }
}