**План занятия:**

- Удалить реализации для Json и Memory хранилища
- Перевести всё приложение на асинхронные методы
- Удалить синхронные методы из интерфейсов репозиториев
- Перевести на асинхронные методы все оставшиеся реализации репозиториев

1. Удаляем реализации для Json и Memory хранилищ

2. Из `IRepository` и `IReadOnlyRepository` удаляем синхронные реализации методов:

    Из `IRepository`:

        - void Update(T item);
        - int Insert(T item);

    Из `IReadOnlyRepository`:

        - IReadOnlyCollection<T> GetAll();
        - int GetCount();
        - T? GetById(int id);

3. Переводим все команды на асинхронную работу:

    Удаляем метод `Execute` из интерфейса `IEshopCommand` на асинхронные методы:

    ```csharp
        public interface IEshopCommand
        {
            public string? Result { get; }
            
            /// <summary>
            /// Выполнить команду
            /// </summary>
            Task ExecuteAsync(string[]? args, CancellationToken cancellationToken);
        }
    ```

     Актуализируем реализую команд на использование нового метода.

     В командах заменяем вызов синхронных методов на асинхронные.

4. Реализуем асинхронные методы для оставшихся репозиториев:

    - BasketDatabaseRepository
    - OrderDatabaseRepository
    - StockDatabaseRepository