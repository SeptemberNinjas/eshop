using eshop.Commands;
using eshop.Commands.CatalogCommands;
using eshop.Commands.OrderCommands;
using eshop.Commands.PaymentCommands;
using eshop.Commands.SystemCommands;
using eshop.Core;
using eshop.DAL;
using eshop.DAL.Json;

namespace eshop;

/// <summary>
/// Контекст сеанса работы приложения
/// </summary>
public class ApplicationContext
{
    /// <summary>
    /// Описание приложения
    /// </summary>
    public const string Title = "Программа: 'Интернет магазин'";

    /// <summary>
    /// Фабрика, для создания репозиторией
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;
    
    public ApplicationContext()
    {
        _repositoryFactory = new JsonRepositoryFactory();
    }

    public IEshopCommand CreateCommand(CommandType commandType)
    {
        return commandType switch
        {
            CommandType.Exit => new ExitCommand(),
            CommandType.Back => new BackCommand(),
            CommandType.GoToRoot => new GoToRootPageCommand(),
            CommandType.DisplaySaleItems => new DisplaySaleItemsCommand(),
            CommandType.DisplayProducts => new DisplayProductsCommand(_repositoryFactory.CreateProductRepository()),
            CommandType.DisplayServices => new DisplayServicesCommand(_repositoryFactory.CreateServiceRepository()),
            CommandType.DisplayBasket => new DisplayBasketCommand(_repositoryFactory.CreateBasketRepository()),
            CommandType.AddProductToBasket => new AddBasketLineCommand(_repositoryFactory.CreateBasketRepository(), (_repositoryFactory.CreateProductRepository() as IRepository<SaleItem>)!),
            CommandType.AddServiceToBasket => new AddBasketLineCommand(_repositoryFactory.CreateBasketRepository(), (_repositoryFactory.CreateServiceRepository() as IReadOnlyRepository<SaleItem>)!),
            CommandType.CreateOrder => new CreateOrderCommand(_repositoryFactory.CreateBasketRepository(), _repositoryFactory.CreateOrdersRepository()),
            CommandType.DisplayOrders => new DisplayOrdersCommand(_repositoryFactory.CreateOrdersRepository()),
            CommandType.StartOrderPayment => new StartOrderPaymentCommand(_repositoryFactory.CreateOrdersRepository()),
            CommandType.SelectPaymentType => new SelectPaymentTypeCommand(),
            CommandType.TransferMoney => new TransferMoneyCommand(_repositoryFactory.CreateOrdersRepository()),
            _ => throw new NotSupportedException()
        };
    }

    public ICommandWithCommandsList GetInitialCommand()
    {
        return new InitialCommand(Title);
    }
}
