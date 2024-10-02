using eshop.Commands;

namespace eshop;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Программа: 'Интернет магазин'");

        while (true)
        {
            Console.WriteLine("Выполните команду");
            var command = Console.ReadLine();
            Execute(command);
        }
    }

}