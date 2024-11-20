**План занятия:**

- Реализовать фронт для нашего магазина

1. Добавляем возможность приложению работать со статичными файлами, для чего в `Program.cs` добавляем `app.UseStaticFiles();` перед `app.UseAuthorization();`\
Если добавить после авторизации, то наша мидлвара с апи ключём не даст конвееру вернуть файл.
2. Добавляем папку `wwwroot` в корень проекта (это директория по умолчанию для статичных файлов, при желании можно настроить другую).
3. Добавляем файл `index.html` в папку `wwwroot`:
    ```html
   <!DOCTYPE html>
   <html lang="ru">
   <head>
       <meta charset="UTF-8">
       <title>EShop</title>   
   </head>
   <body>
   Hello EShop!
   </body>
   </html>
    ```
4. Запускаем приложение, проверяем, что по пути [http://localhost:5064/index.html](http://localhost:5064/index.html) получаем нашу веб страницу.
5. `index.html` является одним из дефолтных имён для статичных фалов. Чтобы приложение переадресовывало с корневого пути на этот файл нужно в `Program.cs` добавить `app.UseDefaultFiles();` перед `app.UseStaticFiles();`.\
Дополнительно в `launchSettings.json` можно изменить дефолтный url для запуска, чтобы не попадать на swagger, или добавить ещё одну конфигурацию для запуска.
   ```json
   {
     "profiles": {
       "site": {
         "commandName": "Project",
         "dotnetRunMessages": true,
         "launchBrowser": true,
         "launchUrl": "",
         "applicationUrl": "http://localhost:5064",
         "environmentVariables": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         }
       }
     }
   }
    ```
6. Запускаем приложение, проверяем, что открывается браузер с нашей веб страницей по корневому пути.
7. Подключим к нашей странице UI фреймворк Bootstrap и обогатим её стилями, используем только стили предоставляемые Bootstrap:
   ```html
   <!DOCTYPE html>
   <html lang="ru">
   <head>
       <meta charset="UTF-8">
       <title>EShop</title>
        <!-- Загружаем стили для библиотеки и иконок с CDN -->
       <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet"
             integrity="sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH" crossorigin="anonymous">
       <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">       
   </head>
   <body>
   <header class="py-3 mb-2 border-bottom bg-primary text-lg-start text-white fs-3 fw-bold">
       <div class="container">
           <div class="d-flex flex-wrap gap-3 align-items-center justify-content-center justify-content-lg-start">
               <div><i class="bi-shop"></i></div>
               <div>EShop</div>
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
   </body>
   </html>
   ```
8. Запускаемся и смотрим результат.
9. Превратим нашу страницу в приложение, дополнив её скриптом, для чего создадим в папке `wwwroot` файл `app.js`:
   ```javascript
   'use strict' 
   // Добавляем эту директиву, чтобы можно было использовать только строгий синтаксис JS
   // и не стрелять себе в ногу 
   // (сборщики добавляют её автоматически, но у нас сбощика нет)
   
   // Выполняем код после загрузки всей страницы, ссылку на скрипт можно будет добавить в любое место)
   window.addEventListener('load', async () => {
       alert('Hello JavaScript!')
   })
   ```
10. Добавляем в `index.html` ссылку на скрипт в начало или конец тега `body`, сразу включаем функционал JSModule с помощью `type="module"` (это позволит использовать import/export без подключения сборщиков):
    ```html
    ...
    <body>
    <script src="app.js" type="module"></script>
    ...
    ```
11. Запускаем приложение, убеждаемся в появлении алерта.
12. Добавим асинхронную загрузку товаров на страницу. Сначала добавим новый js файл `catalog.js`:
   ```javascript
   'use strict'
   
   // Модное объявление функции :) можно объявить так: async function loadCatalog(){...}
   // Помечаем функцию как асинхронную, т.е. возвращающую Promise,
   // т.к. далее будем добавлять в неё асинхронный запрос к апи.
   const loadCatalog = async () => {     
      alert('Hello JSModule import!')
   }
   
   // Экспортируем функцию, чтобы она была доступна для импорта
   export { loadCatalog }
   ```
13. Импортируем функцию `loadCatalog` из `catalog.js` в `app.js`, потом запускаемся и проверяем появление нового алерта из `loadCatalog` (файлы может закешировать, поэтому в браузере юзаем хард резет):
   ```javascript
    'use strict'
    import {loadCatalog} from "./catalog.js" 
    // Расширение файла необходимо указывать, без этого не заработает.
    // Линтер IDE может этому сопротивляться
    
    window.addEventListener('load', async () => {
        await loadCatalog()
    })
   ```
14. Добавляем функцию получения товаров в `catalog.js`, запускаемся и проверяем наличие списка объектов товаров в консоли:
    ```javascript
    const loadProducts = async () => {    
        const productsResponse = await fetch('/Catalog/products', {
            // Нужно добавить заголовок авторизации, иначе наша мидлвара не пустит.
            headers: {
                'Authorization': '123'
            }
        })
        if (productsResponse.status !== 200)
            return []
        
        return await productsResponse.json()
    }
    
    const loadCatalog = async () => {
        // Используем функцию
        const products = await loadProducts()
  
        for (const product of products) {
            console.log(product)
        }
    }
    ```
15. Отобразим полученные товары на странице, для чего дополним `catalog.js`:
    ```javascript
    const loadCatalog = async () => {
        const products = await loadProducts()
    
        const productsContainer = document.querySelector('#products-list')
        if (products.length === 0) {
            productsContainer.innerHTML = 'Товары не найдены'
            return
        }
        for (const product of products) {
            const productElement = document.createElement('div')
            productElement.innerHTML = getProductTemplate(product)
            // Добавим обработчик, чтобы можно было тыкать по товарам
            productElement.addEventListener('click', () => {
                alert(JSON.stringify(product))
            })
            productsContainer.appendChild(productElement)
        }
    }
    
    // Добавим разметку через код
    const getProductTemplate = product => `
        <div class="card eshop-product-card">
            <div class="card-body">${product.name}</div>
            <div class="card-footer d-flex justify-content-between align-items-end">
                <span class="bi-currency-dollar">${product.price}</span>
                <span class="small text-secondary">В наличии: ${product.stock} шт.</span>
            </div>
        </div>
    `
    ```
16. Подкорректируем отображение с помощью своих стилей, для чего в папке `wwwroot` добавим файл `main.css` (стили уже есть в разметке):
    ```css
    .eshop-product-card {
        width: 250px;    
        cursor: pointer;
    }
    .eshop-product-card:hover {
        box-shadow: var(--bs-box-shadow);
    }
    ```
17. Добавим ссылку на `main.css` в `index.html`:
```html
<head>
    ...
    <link rel="stylesheet" href="main.css">
</head>
```
18. Проверяем результат.
