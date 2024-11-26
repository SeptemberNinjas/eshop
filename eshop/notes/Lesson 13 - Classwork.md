**План занятия:**

- Реализовать аутентификацию через куки
- Реализовать привязку корзины к пользователю

1. Удаляем из `Program.cs` регистрацию `ApiKeyMiddleware` и соответствующие настройки swagger.
2. Добавляем схему аутентификации с помощью кук. В `Program.cs` добавить код:
    ```csharp
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options => options.LoginPath = "/login.html");
    ```
3. Добавляем страницу логина `login.html`:
   ```html
   <!DOCTYPE html>
   <html lang="ru">
   <head>
      <meta charset='utf-8'/>
      <title>Eshop Login</title>
      <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet"
            integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
      <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
   </head>
   <body>
   <div class="container w-25 m-auto pt-5">
      <form method='post' action="/login">
         <div class="container d-flex flex-column gap-2 p-3 bg-dark-subtle card">
            <div class="form-group">
               <label for="email">Email</label>
               <input type="email" class="form-control" id="email" name="email" aria-describedby="emailHelp"
                      placeholder="Введите email">
            </div>
            <div class="form-group">
               <label for="password">Пароль</label>
               <input type="password" class="form-control" id="password" name="password" placeholder="Введите пароль">
            </div>
            <button type="submit" class="btn btn-primary">Войти</button>
         </div>
      </form>
   </div>
   </body>
   </html>
   ```
4. Добавляем контроллер аутентификации `AuthenticationController`:
   ```csharp
   using System.Security.Claims;
   using Microsoft.AspNetCore.Authentication;
   using Microsoft.AspNetCore.Authentication.Cookies;
   using Microsoft.AspNetCore.Mvc;
   
   namespace eshop.WebApi.Controllers;
   
   [ApiController]
   [Route("/")]
   public class AuthenticationController : ControllerBase
   {
       [HttpPost("login")]
       public async Task<ActionResult> LoginAsync([FromForm]string email, [FromForm]string password)
       {
           var claims = new List<Claim> { new(ClaimTypes.Name, email) };
           var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
           
           await  HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
       
           return Redirect("/");
       }
   
       [HttpPost("logout")]
       public async Task<ActionResult> LogoutAsync()
       {
           await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
           return Redirect("/");
       }
   }
   ```
5. Добавим на страницу `index.html` кнопки входы и выхода:
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
   <script src="app.js" type="module"></script>
   <header class="py-3 mb-2 border-bottom bg-primary text-lg-start text-white fs-3 fw-bold">
       <div class="container">
           <div class="d-flex flex-wrap gap-3 align-items-center justify-content-center justify-content-between">
               <div class="d-flex flex-wrap gap-3 align-items-center justify-content-center justify-content-lg-start">
               <div><i class="bi-shop"></i></div>
               <div>EShop</div>
               </div>
               <div>
               <a href="/login.html"><div class="btn btn-light">Войти</div></a>
               <div class="btn btn-light" onclick="document.querySelector('#logout').submit()">Выйти</div>
               </div>
           </div>
       </div>
   </header>
   <div class="bg-body">
       <div class="container bg-dark-subtle p-4">
           <div class="bg-dark-subtle fs-5 fw-bold text-uppercase">Товары</div>
           <hr/>
           <div id="products-list" class="d-flex gap-2"></div>
       </div>
   </div>
   <form id="logout" style="display: none" action="/logout" method="post"></form>
   </body>
   </html>
   ```
6. Запустим проект, попробуем работу формы. Добавим атрибут `Authorize` на метод получения товаров, убедимся, что товары перестают отображаться на странице, если пользователь не авторизован. Уберем атрибут.
7. Свяжем корзину с авторизованным пользователем. По хорошему нужно ввести сущность покупателя, но пока не хочется сильно усложнять, поэтому просто в таблицу корзины добавим столбец с именем покупателя.
   ```sql
   create table basket (    
       id serial primary key,
       customer varchar(128) not null UNIQUE
   );
   ```
8. Доработаем корзину для возможности работы по покупателю:
   ```csharp
   // Basket.cs
   // Добавляем свойство с логином покупателя
   public string Customer { get; }
   
   // Добавим инициализацию свойства в существующие конструкторы
   public Basket(string customer)
   {
     Customer = customer;
     _lines = [];
   }
   
   public Basket(int id, IEnumerable<ItemsListLine> lines, string customer)
   {
     Id = id;
     Customer = customer;
     _lines = lines.ToList();
   }
   ```
9. Доработаем репозиторий
   ```csharp
   // Добавим сущность BasketRow для удобства выборки данных по корзине
   public record BasketRow(int Id, string Customer, ItemsListLine? Item);
   
   // BasketDatabaseRepository.cs
   private static BasketRow GetBasketLine(DbDataReader reader)
   {
     var basketId = reader.GetFieldValue<int>("basket_id");
     var customer = reader.GetFieldValue<string>("customer");
     var itemId = reader.GetFieldValue<int?>("item_id");
     if (itemId is null)
         return new BasketRow(basketId, customer, null);
     var itemType = (ItemTypes)reader.GetFieldValue<int>("type");
     SaleItem item = itemType == ItemTypes.Product
         ? new Product(reader.GetFieldValue<int>("item_id"),
             reader.GetFieldValue<string>("name"),
             reader.GetFieldValue<decimal>("price"),
             reader.GetFieldValue<int>("amount"))
         : new Service(reader.GetFieldValue<int>("item_id"),
             reader.GetFieldValue<string>("name"),
             reader.GetFieldValue<decimal>("price"));
   
     return new BasketRow(basketId, customer, new ItemsListLine(item, reader.GetFieldValue<int>("count")));
   }
   
   public async Task<Basket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
   {
     var commandText = 
         $"""
          select b.id as basket_id, b.customer, c.id as item_id, c.price, c."name", c."type" , s.amount , bl.count
          from basket b
          left join basket_line bl on b.id = bl.basket_id
          left join catalog c on bl.item_id = c.id
          left join stock s on c.id = s.id
          where b.id = {id}
          """;
   
     var result = await ExecuteReaderListAsync(commandText, GetBasketLine, cancellationToken);
   
     return result.Count == 0
         ? null 
         : new Basket(result.First().Id, result.Select(i => i.Item)!, result.First().Customer);
   }
   
   public async Task<int> InsertAsync(Basket item, CancellationToken cancellationToken)
   {
     await using var command = GetCommand(
         $"""
          insert into basket(customer) values ('{item.Customer}')
          """);
     return await command.ExecuteNonQueryAsync(cancellationToken);
   }
   
   public async Task<IReadOnlyCollection<Basket>> GetAllAsync(CancellationToken cancellationToken = default)
   {
     var commandText = 
         $"""
          select b.id as basket_id, b.customer, c.id as item_id, c.price, c."name", c."type" , s.amount , bl.count
          from basket b
          left join basket_line bl on b.id = bl.basket_id
          left join catalog c on bl.item_id = c.id
          left join stock s on c.id = s.id
          """;
   
     var result = await ExecuteReaderListAsync(commandText, GetBasketLine, cancellationToken);
     return result
         .GroupBy(r => r.Id)
         .Select(g => new Basket(g.Key, g
             .Where(i => i.Item is not null)
             .Select(i => i.Item!), g.First().Customer))
         .ToArray();
   }
   ```
10. Дорабатываем обработчики корзины:
   ```csharp
   // GetBasketHandler.cs
   public async Task<Result<Basket>> GetBasketAsync(string customer, CancellationToken cancellationToken)
   {
      try
      {
          var repository = _repositoryFactory.CreateBasketRepository();
          var baskets = await repository.GetAllAsync(cancellationToken);
          var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
          if (customerBasket is null)
              return Result.Fail($"Корзина покупателя с логином {customer} не найдена");
          
          return Result.Ok(customerBasket);
      }
      catch (Exception ex)
      {
          return Result.Fail("Не удалось получить корзину")
              .WithError(ex.Message)
              .WithError(ex.StackTrace);
      }
   }
   // ClearBasketHandler.cs
   public async Task<Result> ClearBasketAsync(string customer, CancellationToken cancellationToken)
   {
     try
     {
         var repository = _repositoryFactory.CreateBasketRepository();
         var baskets = await repository.GetAllAsync(cancellationToken);
         var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
         if (customerBasket is null)
             return Result.Fail("Корзина не найдена");
         
         customerBasket.Clear();
         await repository.UpdateAsync(customerBasket, cancellationToken);
             
         return Result.Ok()
             .WithSuccess("Корзина очищена");
     }
     catch (Exception ex)
     {
         return Result.Fail("Не удалось очистить корзину")
             .WithError(ex.Message)
             .WithError(ex.StackTrace);
     }
   }
   // AddBasketLineHandler.cs
   public async Task<Result> AddLineAsync(string customer, int itemId, int count,
      CancellationToken cancellationToken)
   {
      try
      {
          var basketRepository = _repositoryFactory.CreateBasketRepository();
          
          var baskets = await basketRepository.GetAllAsync(cancellationToken);
          var customerBasket = baskets.FirstOrDefault(b => b.Customer == customer);
          if (customerBasket is null)
          {
              await basketRepository.InsertAsync(new Basket(customer), cancellationToken);
              customerBasket = (await basketRepository.GetAllAsync(cancellationToken))
                  .FirstOrDefault(b => b.Customer == customer);
          }
          if (customerBasket is null)
              return Result.Fail("Корзина не найдена");
   
          var itemsRepository = _repositoryFactory.CreateSaleItemRepository();
          var item = await itemsRepository.GetByIdAsync(itemId, cancellationToken);
          if (item is null)
              return Result.Fail("Товар или услуга не найдены");
    
          var result = item switch
          {
              Product product => await AddLineAsync(product, count, customerBasket, basketRepository),
              Service service => AddLine(service, customerBasket),
              _ => Result.Fail("Неизвестный тип товарной единицы")
          };
   
          if (result.IsSuccess)
              await basketRepository.UpdateAsync(result.Value, cancellationToken);
   
          return result.ToResult();
      }
      catch (Exception ex)
      {
          return Result.Fail("Не удалось получить корзину")
              .WithError(ex.Message)
              .WithError(ex.StackTrace);
      }
   }
   ```
11. Сломались команды тесты и контроллер корзины. Чиним команды прокидыванием пустой строки, тесты на занятии чиним, если хватит времени.
12. Ставим атрибут `Authorize` на контроллер корзины, получаем в методах имя пользователя `var customer = User.Identity?.Name` и прокидываем в обработчики.
13. Запускаем проект, пробуем создавать и смотреть корзину через с сваггер с авторизацией и без.