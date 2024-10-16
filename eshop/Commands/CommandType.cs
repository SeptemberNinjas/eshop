namespace eshop.Commands;

public enum CommandType
{
    DisplaySaleItems,
    DisplayProducts,
    DisplayServices,
    DisplayBasket,
    AddProductToBasket,
    AddServiceToBasket,
    CreateOrder,
    DisplayOrders,
    StartOrderPayment,
    SelectPaymentType,
    TransferMoney,
    GoToRoot,
    Back,
    Exit
}