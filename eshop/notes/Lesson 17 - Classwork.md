**План занятия:**
- Реализация оплаты через httpclient и второе веб-приложение, реализованное в ДЗ № 10.
- Реализовать UI для оформления и отображения заказов.

1. Подготавливаем и проверяем работоспособность нашего приложения "банка". В проект у нас добавлено приложение `eshop.BankApi`
2. Зарегистрируем в DI Http клиент для сервиса банка, адрес вынесем в настройки.
    ```json
    "PaymentGateway": "http://localhost:5238"
    ```
    ```csharp
    // Program.cs
    builder.Services.AddHttpClient<PayOrderByCashlessHandler>(h =>
    {
        var baseAddress = builder.Configuration["PaymentGateway"];
        if (baseAddress is null)
            throw new ApplicationException("Не указаны настройки сервиса оплаты");
        h.BaseAddress = new Uri(baseAddress);
    });
    ```
3. Тут вероятно нужно рассказать по созданию экземпляров HttpClient и проблем с этим связанных, если делать это не правильно.\
Вот тут большая часть компактно описана [ссылочка на статью](https://learn.microsoft.com/ru-ru/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net)
4. Добавим дто для запросов в сервис банка
    ```csharp
    namespace eshop.Application.Payment;
    
    public record PaymentRequest(int OrderId, decimal Amount);
    ```
    ```csharp
    using System.Text.Json.Serialization;
    
    namespace eshop.Application.Payment;
    
    [Serializable]
    public record PaymentResult(
        [property: JsonPropertyName("isSuccess"), JsonRequired]
        bool IsSuccess, 
        [property: JsonPropertyName("message"), JsonRequired]
        string Message);
    ```
   ```csharp
   namespace eshop.WebApi.Responses;
   
   public record PaymentResponse(bool IsSuccess, string Message);
   ```
5. Доработаем контракт обработчиков оплаты и соответствующий контроллер (кроме безнала, его доработаем следующим шагом)
   ```csharp
   using eshop.Core;
   using FluentResults;
   using Microsoft.Extensions.Logging;
   
   namespace eshop.Application.Payment
   {
       public abstract class PayOrderBaseHandler
       {
           private readonly IRepository<Core.Order> _ordersRepository;
           private readonly ILogger<PayOrderBaseHandler> _logger;
   
           public PayOrderBaseHandler(IRepository<Core.Order> ordersRepository, ILogger<PayOrderBaseHandler> logger)
           {
              
               _ordersRepository = ordersRepository;
               _logger = logger;
           }
   
           protected async Task<Result<PaymentResult>> PayAsync(int orderId, decimal amount, CancellationToken cancellationToken)
           {
               try
               {
                   var order = await _ordersRepository.GetByIdAsync(orderId, cancellationToken);
   
                   if (order == null)
                       return Result.Fail($"Заказ с идентификатором {orderId} не найден");
   
                   if (order.Status is not OrderStatus.New)
                       return Result.Fail($"Заказ с идентификатором {orderId} нельзя оплатить");
   
                   var result = await PaymentProcessingAsync(order, amount);
   
                   if (result.IsFailed)
                       return Result.Fail("Не удалось выполнить оплату")
                           .WithErrors(result.Errors);
   
                   if (result.Value.IsSuccess && order.SetPaidStatus())
                   {
                       await _ordersRepository.UpdateAsync(order, cancellationToken);
                   }
   
                   return result;
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Ошибка при оплате заказа. {message}", ex.Message);
                   
                   return Result.Fail("Не удалось оплатить заказ");
               }
           }
   
           protected abstract Task<Result<PaymentResult>> PaymentProcessingAsync(Core.Order order, decimal amount);
       }
   }
   ```
   ```csharp
   using eshop.Core;
   using FluentResults;
   using Microsoft.Extensions.Logging;
   
   namespace eshop.Application.Payment
   {
       public class PayOrderByCashHandler : PayOrderBaseHandler
       {
           public PayOrderByCashHandler(IRepository<Core.Order> ordersRepository, ILogger<PayOrderByCashHandler> logger)
               : base(ordersRepository, logger)
           {
           }
   
           public async Task<Result<PaymentResult>> PayHandler(int orderId, decimal amount, CancellationToken cancellationToken)
           {
               return await PayAsync(orderId, amount, cancellationToken);
           }
   
           protected override Task<Result<PaymentResult>> PaymentProcessingAsync(Core.Order order, decimal amount)
           {
               var sum = order.Sum;
   
               if (amount < sum)
                   return Task.FromResult<Result<PaymentResult>>(Result.Fail("Недостаточно средств"));
   
               if (amount == sum)
                   return Task.FromResult(Result.Ok(new PaymentResult(true, "Заказ оплачен")));
   
               return Task.FromResult(Result.Ok(new PaymentResult(true, $"Заказ оплачен, сдача {amount - sum:F2}")));
           }
       }
   }
   ```
   ```csharp
   using eshop.Application.Payment;
   using eshop.WebApi.Responses;
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Mvc;
   using PaymentRequest = eshop.WebApi.Requests.PaymentRequest;
   
   namespace eshop.WebApi.Controllers
   {
       [Route("[controller]")]
       [ApiController]
       [Authorize]
       public class PaymentController : ControllerBase
       {
           private readonly PayOrderByCashHandler _payByCashHandler;
           private readonly PayOrderByCashlessHandler _payOrderByCashlessHandler;
   
           public PaymentController(
               PayOrderByCashHandler payByCashHandler,
               PayOrderByCashlessHandler payOrderByCashlessHandler)
           {
               _payByCashHandler = payByCashHandler;
               _payOrderByCashlessHandler = payOrderByCashlessHandler;
           }
   
           [HttpPatch]
           public async Task<ActionResult<PaymentResponse>> PayAsync(PaymentRequest request, CancellationToken cancellationToken)
           {
               var  result = request.PaymentType == 1 ?
                   await _payByCashHandler.PayHandler(request.OrderId, request.Amount, cancellationToken) :
                   await _payOrderByCashlessHandler.PayHandler(request.OrderId, request.Amount, cancellationToken);
   
               var isSuccess = result.IsSuccess && result.Value.IsSuccess;
               var message = result.IsSuccess ? result.Value.Message : result.ToString();
   
               return Ok(new PaymentResponse(isSuccess, message));
           }
       }
   }
   ```
6. Добавим в обработчик безналичной оплаты обращение через http клиента в апи банка
   ```csharp
   using System.Net;
   using System.Net.Http.Headers;
   using System.Text.Json;
   using eshop.Core;
   using FluentResults;
   using Microsoft.Extensions.Logging;
   
   namespace eshop.Application.Payment
   {
       public class PayOrderByCashlessHandler : PayOrderBaseHandler
       {
           private readonly ILogger<PayOrderByCashlessHandler> _logger;
           private readonly HttpClient _client;
   
           public PayOrderByCashlessHandler(IRepository<Core.Order> orderRepository, ILogger<PayOrderByCashlessHandler> logger, HttpClient client) 
               : base(orderRepository, logger)
           {
               _logger = logger;
               _client = client;
           }
   
           public async Task<Result<PaymentResult>> PayHandler(int orderId, decimal amount, CancellationToken cancellationToken)
           {
               return await PayAsync(orderId, amount, cancellationToken);
           }
   
           protected override async Task<Result<PaymentResult>> PaymentProcessingAsync(Core.Order order, decimal amount)
           {
               var sum = order.Sum;
   
               if (amount < sum)
                   return Result.Fail("Недостаточно средств");
   
               if (amount != sum)
                   return Result.Fail($"Внесите ровно {sum}, при безналичном платеже переплата не допускается.");
   
               var requestBody = new PaymentRequest(order.Id, amount);
               var request = new HttpRequestMessage(HttpMethod.Post, "Payment");
               request.Content = new StringContent(JsonSerializer.Serialize(requestBody));
               request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
               
               var response = await _client.SendAsync(request);
               if (response.StatusCode != HttpStatusCode.OK)
               {
                   const string message = "Ошибка при выполнении оплаты";
                   using var _ = _logger.BeginScope(new Dictionary<string, object> {{"response", response}});
                   _logger.LogError(message);
                   return Result.Fail(message);
               }
   
               var content = await response.Content.ReadAsStringAsync();
               var result = JsonSerializer.Deserialize<PaymentResult>(content);
               if (result is null)
                   return Result.Fail("Не удалось прочитать ответ");
               
               return Result.Ok(result);
           }
       }
   }
   ```
7. Запускаем оба проекта и проверяем через swagger, что теперь безналичные оплаты идут через внешний сервис.
8. Доработаем UI для работы с заказами и оплатами.\
Отобразим заказы на странице корзины, но для этого необходимо привязать заказы к покупателю.\
9. Создадим таблицу заказов покупателей
   ```sql
   create table customer_orders (
        customer varchar(128) not null,
        order_id int not null UNIQUE
   );
   ```
10. Добавим репозиторий для работы с заказами покупателя
   ```csharp
   using LinqToDB.Mapping;
   
   namespace eshop.DAL.LinqToDb.Schema;
   
   [Table(Name = "customer_orders")]
   public class CustomerOrders
   {
       [Column("customer", Length = 128), NotNull]
       public string? Customer { get; set; }
       
       [Column("order_id"), NotNull]
       public int OrderId { get; set; }
       
       [Association(ThisKey = "OrderId", OtherKey = "Id", CanBeNull = false)] // ассоциации предоставляют упрощенную реаизацию для inner join
       public IEnumerable<OrderRow>? Orders { get; set; }
   }
   ```
   ```csharp
   // LinqToDbContext.cs Добавим таблицу в контекст
   public ITable<CustomerOrders> CustomerOrders => this.GetTable<CustomerOrders>();
   ```
   ```csharp
   // OrdersLinqToDbRepository.cs 
   // чтобы переиспользовать метод маппинга сделаем его internal.
   internal static Order MapOrder(OrderRow order)...
   ```
   ```csharp
   namespace eshop.Core;
   
   public interface ICustomerOrdersRepository
   {
       Task LinkOrderToCustomerAsync(string customer, int orderId, CancellationToken cancellationToken);
       
       Task<List<Order>> GetCustomerOrdersAsync(string customer, CancellationToken cancellationToken);
   }
   ```
   ```csharp
   using eshop.Core;
   using eshop.DAL.LinqToDb.Schema;
   using LinqToDB;
   
   namespace eshop.DAL.LinqToDb
   {
       public class CustomerOrdersLinqToDbRepository : ICustomerOrdersRepository
       {
           private readonly LinqToDbContext _context;
   
           public CustomerOrdersLinqToDbRepository(LinqToDbContext context)
           {
               _context = context;
           }
   
           public async Task LinkOrderToCustomerAsync(string customer, int orderId, CancellationToken cancellationToken)
           {
               await _context.CustomerOrders.InsertAsync(() => new CustomerOrders
               {
                   Customer = customer,
                   OrderId = orderId
               }, token: cancellationToken);
           }
   
           public async Task<List<Order>> GetCustomerOrdersAsync(string customer, CancellationToken cancellationToken)
           {
               return await _context.CustomerOrders
                   .LoadWith(r => r.Orders) // говорим orm сделать join по ассоциации
                   .ThenLoad(o => o.Lines) // то же самое для вложенных ассоциаций
                   .ThenLoad(l => l.Item)
                   .Where(r => r.Customer == customer)
                   .SelectMany(r => r.Orders!)
                   .Select(r => OrdersLinqToDbRepository.MapOrder(r))
                   .ToListAsync(cancellationToken);
           }
       }
   }
   ```
   ```csharp
   // ConfigurationExtensions.cs
   services.AddScoped<ICustomerOrdersRepository, CustomerOrdersLinqToDbRepository>();
   ```
11. Доработаем обработчик создания заказа на использование нового репозитория
   ```csharp
   using eshop.Core;
   using eshop.Core.Cache;
   using eshop.DAL.Database;
   using FluentResults;
   using Microsoft.Extensions.Logging;
   
   namespace eshop.Application.Order;
   
   public class CreateOrderHandler
   {
       private readonly DatabaseContext _databaseContext;
       private readonly IRepository<Core.Order> _ordersRepository;
       private readonly IRepository<Basket> _basketRepository;
       private readonly IRepository<Stock> _stockRepository;
       private readonly ILogger<CreateOrderHandler> _logger;
       private readonly CacheKeysStorage _keysStorage;
       private readonly ICustomerOrdersRepository _customerOrdersRepository;
   
       public CreateOrderHandler(
           DatabaseContext databaseContext,
           IRepository<Core.Order> ordersRepository,
           IRepository<Basket> basketRepository,
           IRepository<Stock> stockRepository,
           ILogger<CreateOrderHandler> logger,
           CacheKeysStorage keysStorage, 
           ICustomerOrdersRepository customerOrdersRepository)
       {
           _databaseContext = databaseContext;
           _ordersRepository = ordersRepository;
           _basketRepository = basketRepository;
           _logger = logger;
           _keysStorage = keysStorage;
           _customerOrdersRepository = customerOrdersRepository; // Добавили в обработчик
           _stockRepository = stockRepository;
       }
   
       public async Task<Result> CreateOrderAsync(string customer, CancellationToken cancellationToken)
       {
           try
           {
               await _databaseContext.BeginTransactionAsync(cancellationToken);
               var currentBasket = (await _basketRepository.GetAllAsync(cancellationToken))
                   .FirstOrDefault(b => b.Customer == customer);
               if (currentBasket is null || currentBasket.Lines.Count == 0)
                   return Result.Fail("Корзина не найдена");
     
               var order = currentBasket.CreateOrderFromBasket();
               if (order is null)
                   return Result.Fail("Ошибка при создании заказа. Корзина пуста");
               
               var orderedProductsWithCount = order.Lines
                   .Where(l => l.ItemType is ItemTypes.Product)
                   .Join(await _stockRepository.GetAllAsync(cancellationToken),
                       orderLine => orderLine.ItemId, 
                       stock => stock.ItemId,
                       (orderLine, stock) => (stock, orderLine.Count));                
              
               var id = await _ordersRepository.InsertAsync(order, cancellationToken);
               await _basketRepository.UpdateAsync(currentBasket, cancellationToken);
               foreach (var (stock, count) in orderedProductsWithCount)
               {
                   if (stock.Amount - count < 0)
                   {
                       await _databaseContext.RollbackTransactionAsync(cancellationToken);
                       return Result.Fail("Недостаточно товара");
                   }
   
                   stock.Amount -= count;
                   await _stockRepository.UpdateAsync(stock, cancellationToken);
               }
               
               // Привязали заказ к покупателю
               await _customerOrdersRepository.LinkOrderToCustomerAsync(customer, id, cancellationToken);
   
               await _databaseContext.CommitTransactionAsync(cancellationToken);
               _keysStorage.RemoveGroupCache(CacheKeysStorage.SaleItemsGroup);
               
               return Result.Ok()
                   .WithSuccess($"Создан заказ {id}");
           }
           catch (Exception ex)
           {
               await _databaseContext.RollbackTransactionAsync(cancellationToken);
               _logger.LogError(ex, "Ошибка при создании заказа. {message}", ex.Message);
               
               return Result.Fail("Не удалось создать заказ");
           }
       }
   }
   ```
12. Доработаем обработчик получения заказов на использование нового репозитория, а также создадим DTO для отображения заказа
   ```csharp
   using eshop.Core;
   
   namespace eshop.Application.Order;
   
   public record OrderDto(int Id, OrderStatus Status, decimal TotalSum, IEnumerable<OrderItemDto> Items);
   ```
   ```csharp
   using eshop.Core;
   
   namespace eshop.Application.Order;
   
   public record OrderItemDto(int Id, ItemTypes ItemType, string Name, int Amount, decimal Price, decimal Sum);
   ```
   ```csharp
   using eshop.Core;
   using FluentResults;
   using Microsoft.Extensions.Logging;
   
   namespace eshop.Application.Order;
   
   public class GetOrdersHandler
   {
       private readonly ICustomerOrdersRepository _ordersRepository;
       private readonly ILogger<GetOrdersHandler> _logger;
   
       public GetOrdersHandler(ICustomerOrdersRepository ordersRepository, ILogger<GetOrdersHandler> logger)
       {
           _ordersRepository = ordersRepository;
           _logger = logger;
       }
   
       public async Task<Result<IEnumerable<OrderDto>>> GetOrdersAsync(string customer, CancellationToken cancellationToken)
       {
           try
           {
               var orders = await _ordersRepository.GetCustomerOrdersAsync(customer, cancellationToken);
               return Result.Ok(orders.Select(Map));
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Ошибка при получении заказов. {message}", ex.Message);
               
               return Result.Fail("Не удалось получить список заказов");
           }
       }
       
       private static OrderDto Map(Core.Order order)
       {
           var totalSum = order.Lines.Sum(l => l.SaleItem.Price * l.Count);
           return new OrderDto(order.Id, order.Status, totalSum, order.Lines
               .Select(l =>
                   new OrderItemDto(l.ItemId, l.ItemType, l.SaleItem.Name, l.Count, l.SaleItem.Price,
                       l.Count * l.SaleItem.Price)));
       }
   }
   ```
13. Доработаем фронт для работы с заказами. добавим в разметку `basket.html` контейнер для списка заказов
   ```html
   <!DOCTYPE html>
   <html lang="ru">
   <head>
       <meta charset="UTF-8">
       <title>EShop</title>
       <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet"
             integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
       <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
       <link rel="stylesheet" href="main.css">
   </head>
   <body>
   <script src="basket-page.js" type="module"></script>
   <header class="py-3 mb-2 border-bottom bg-primary text-lg-start text-white fs-3 fw-bold">
       <div class="container">
           <div class="d-flex flex-wrap gap-3 align-items-center justify-content-center justify-content-between">
               <div class="pointer-cursor d-flex flex-wrap gap-3 align-items-center justify-content-center justify-content-lg-start"
                    onclick="window.location = '/'">
                   <div><i class="bi-shop"></i></div>
                   <div>EShop - Корзина</div>
               </div>
               <div>
                   <a href="/login.html">
                       <div class="btn btn-light">Войти</div>
                   </a>
                   <div class="btn btn-light" onclick="document.querySelector('#logout').submit()">Выйти</div>
               </div>
           </div>
       </div>
   </header>
   <div class="bg-body">
       <div class="container bg-dark-subtle p-4">
           <div class="bg-dark-subtle fs-5 fw-bold d-flex justify-content-between">
               <div>
                   Покупатель - <span id="customer-name"></span>
               </div>
               <div>
                   <button id="clear-basket" type="button" class="btn btn-dark">Очистить корзину</button>
                   <button id="create-order" type="button" class="btn btn-dark">Оформить заказ</button>
               </div>            
           </div>
           <hr/>
           <div class="bg-dark-subtle fs-5 fw-bold d-flex justify-content-between pb-1">Корзина</div>
           <table id="basket-items-list" class="table table-hover">
           </table>
       </div>
       <div id="orders-container" class="container bg-dark-subtle p-4">
           <div class="bg-dark-subtle fs-5 fw-bold d-flex justify-content-between">
               Заказы           
           </div>
           <hr/>
           <div id="orders-list">
           </div>
       </div>
   </div>
   <form id="logout" style="display: none" action="/logout" method="post"></form>
   </body>
   </html>
   ```
14. Вынесем общие для корзины и заказа шаблоны разметки в отдельный файл.
   ```javascript
   // templates.js
   'use strict'
   
   export const getTableItemTemplate = (item, index) => `
       <tr>
           <th scope="row">${index}</th>
           <td>${item.name}</td>
           <td>${item.price}</td>
           <td>${item.amount}</td>
           <td>${item.sum}</td>
       </tr>
   `
   
   export const getTableHeadTemplate = () => `
       <thead>
           <tr class="table-dark">
               <th scope="col">#</th>
               <th scope="col">Наименование</th>
               <th scope="col">Цена</th>
               <th scope="col">Количество</th>
               <th scope="col">Сумма</th>
           </tr>
       </thead>
   `
   
   export const getTotalLineTemplate = totalSum => `
       <thead>
           <tr class="info">
               <th scope="row" colspan="4" class="text-end">Итого:</th>            
               <td>${totalSum}</td>
           </tr>
       </thead>
   `
   ```
   ```javascript
   // basket.js
   'use strict'
   
   export const loadBasket = async () => {
      const basketResponse = await fetch('/Basket')
      if (basketResponse.redirected){
         window.location = basketResponse.url
         return
      }
   
      return await basketResponse.json()
   }
   
   export const clearBasket = async () => {
      const basketResponse = await fetch('/Basket', {
         method: 'delete'
      })
   
      if (basketResponse.redirected){
         window.location = basketResponse.url
         return
      }
   
      return basketResponse.status === 200 ? 'Корзина очищена' : undefined
   }
   
   export const addBasketLine = async (item) => {
      const basketResponse = await fetch('/Basket/line', {
         method: 'patch',
         headers: {
            'Content-Type': 'application/json'
         },
         body: JSON.stringify({
            id: item.id,
            count: 1
         })
      })
   
      if (basketResponse.redirected){
         window.location = basketResponse.url
      }
   }
   
   export const getBasketSize = async () => {
      const basketResponse = await fetch('/Basket')
      if (basketResponse.redirected || basketResponse.status !== 200){
         return 0;
      }
   
      return (await basketResponse.json())?.items?.length ?? 0
   }
   ```
15. Добавим функции для работы с заказами
   ```javascript
   // orders.js
   'use strict'
   
   export const loadOrders = async () => {
       const response = await fetch('/Order')
       if (response.redirected) {
           window.location = response.url
           return
       }
       if (response.status !== 200)
           return []
   
       return await response.json()
   }
   
   export const createOrder = async () => {
       const response = await fetch('/Order', {
           method: 'post'
       })
   
       if (response.redirected)
           window.location = response.url
       
       window.location.reload()
   }
   
   const orderStatusMap = new Map()
   orderStatusMap.set(0, 'Не оплачен')
   orderStatusMap.set(1, 'Оплачен')
   
   export const getOrderTemplate = (order, index) => `
       <div id="order-${order.id}" class="container bg-dark-subtle p-4 d-flex flex-column gap-3">
           <div class="bg-dark-subtle fs-5 fw-bold d-flex justify-content-between">                   
               <div><span>${index}.&nbsp;</span><span class="message-container">Статус - <span>${orderStatusMap.get(order.status)}</span></span></div>            
               <button type="button" class="pay-button btn btn-dark">Оплатить</button>
           </div>
           <table class="order-items-list table table-hover">
           </table>
       </div>
   `
   ```
16. Добавим новый функционал в скрипт страницы
   ```javascript
   // basket-page.js
   'use strict'
   import {clearBasket, loadBasket} from "./basket.js";
   import {createOrder, getOrderTemplate, loadOrders} from "./orders.js";
   import {getTableHeadTemplate, getTableItemTemplate, getTotalLineTemplate} from "./templates.js";
   
   window.addEventListener('load', async () => {
       await drawBasket()
       await drawOrders()    
   })
   
   async function drawBasket() {
       const basket = await loadBasket()
       document.querySelector('#customer-name').innerHTML = basket.customerName
       const itemsListContainer = document.querySelector('#basket-items-list')
       const clearButton = document.querySelector('#clear-basket')
       const createOrderButton = document.querySelector('#create-order')
   
       if (basket.items && basket.items.length) {
           itemsListContainer.innerHTML = getTableHeadTemplate()
           for (let i = 0; i < basket.items.length; i++) {
               itemsListContainer.innerHTML += getTableItemTemplate(basket.items[i], i + 1)
           }
           itemsListContainer.innerHTML += getTotalLineTemplate(basket.totalSum)
       } else {
           clearButton.setAttribute('style', 'display:none')
           createOrderButton.setAttribute('style', 'display:none')
           itemsListContainer.innerHTML = 'Корзина пуста'
       }
   
       clearButton.addEventListener('click', async () => {
           const clearResult = await clearBasket()
           if (!clearResult)
               return
   
           itemsListContainer.innerHTML = clearResult
           clearButton.setAttribute('style', 'display:none')
           createOrderButton.setAttribute('style', 'display:none')
       })
   
       createOrderButton.addEventListener('click', async () => {
           await createOrder()       
       })
   }
   
   async function drawOrders() {
       const orders = await loadOrders()
       const container = document.querySelector('#orders-list')
       orders?.forEach((o, i) => drawOrder(container, o , i))
   }
   
   const drawOrder = (container, order, index)  => {  
       const template = getOrderTemplate(order, index + 1)
       const div = document.createElement('div');
       div.innerHTML = template.trim();
       container.appendChild(div.firstChild)    
       const orderElement = container.querySelector(`#order-${order.id}`)
       const itemsListContainer = orderElement.querySelector('.order-items-list')
       const payButton = orderElement.querySelector('.pay-button')
       if (order.status > 0)
           payButton.setAttribute('style', 'display: none')
       
       payButton.addEventListener('click', async () => {
           const response = await fetch('Payment', {
               method: 'patch',
               headers: {
                   'Content-Type': 'application/json'
               },
               body: JSON.stringify({
                   orderId: order.id,
                   paymentType: 2,
                   amount: order.totalSum
               })
           })
           if (response.status !== 200)
               return
           
           const result = await response.json()
           
           if (result.isSuccess){
               payButton.setAttribute('style', 'display: none')
           }
           
           orderElement.querySelector('.message-container').innerHTML = result.message
       })
   
       if (order.items && order.items.length) {
           itemsListContainer.innerHTML = getTableHeadTemplate()
           for (let i = 0; i < order.items.length; i++) {
               itemsListContainer.innerHTML += getTableItemTemplate(order.items[i], i + 1)
           }
           itemsListContainer.innerHTML += getTotalLineTemplate(order.totalSum)
       }
   }
   ```
17. Запускаем и смотрим результат.