**План занятия:**

- Выделить базовый класс "Торговая единица"

Пишем код:

1. Создаем новый абстрактный класс SaleItem

    ```csharp
        /// <summary>
        /// Торговая единица
        /// </summary>
        public abstract class SaleItem
        {
            /// <summary>
            /// Идентификатор
            /// </summary>
            public int Id { get; init; }

            /// <summary>
            /// Наименование
            /// </summary>
            public string Name { get; init; }

            /// <summary>
            /// Цена
            /// </summary>
            public decimal Price { get; init; }

            public SaleItem(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
    ```

2. Наследуем `Product` и `Service` от от `SaleItem`

    - из Product удаляем свойства Id, Name, Price
    - добавляем вызов базового конструктора

3. В SaleItem объявляем абстрактный метод `GetDisplayText` и его реализации в `Product` и `Service`

    ```csharp: SaleItem
        /// <summary>
        /// Текстовое описание торговой единицы
        /// </summary>
        /// <returns></returns>
        public abstract string GetDisplayText();
    ```

    ```csharp: Product
        /// <inheritdoc/>
        public override string GetDisplayText()
        {
            return $"{Id}. {Name}. Цена: {Price:F2}. Остаток: {Stock}";
        }
    ```

    ```csharp: Service
        /// <inheritdoc/>
        public override string GetDisplayText()
        {
            return $"{Id}. {Name}. Цена: {Price:F2}";
        }
    ```

4. Реализуем обобщенный тип `ItemsListLine`

    Удаляем поля для `Product` и `Service` и добавляем одно поле `_lineItem`
    Правим реализацию свойств `ItemId`, `ItemType`, `Text`, `LineSum`
    Удаляем отдельный конструкторы для `Product` и `Service` и добавляем единый новый конструктор

    ```csharp
        /// <summary>
        /// Линия списка торговой единицы
        /// </summary>
        public class ItemsListLine<T> where T : SaleItem
        {
            private readonly T _lineItem;
            private int _count;

            ...

            /// <summary>
            /// Идентификатор элемента
            /// </summary>
            public int ItemId => _lineItem.Id;

            /// <summary>
            /// Тип элемента
            /// </summary>
            public ItemTypes ItemType => _lineItem is Product ? ItemTypes.Product : ItemTypes.Service;

            /// <summary>
            /// Текст, отображаемый в списке элементов
            /// </summary>
            public string Text => $"{ItemType}: {_lineItem?.Name} | Цена: {_lineItem?.Price:F2} | Кол-во: {Count}";

            ...

            /// <summary>
            /// Суммарная стоимость по линии
            /// </summary>
            public decimal LineSum => (_lineItem?.Price ?? 0) * Count;

            /// <inheritdoc cref="ItemsListLine"/>
            public ItemsListLine(T lineItem, int requestedCount)
            {
                _lineItem = lineItem;
                Count = requestedCount;
            }
        }
    ```

    Добавляем класс для реализации конструкторов создания линий с соответствующими типами торговой единицы (вместо реализации фабрики, которая оставлена на домашнее задание).

    Особенность заключается в том, что класс `ItemsListLine` и `ItemsListLine<SaleItem>` это два разных типа, несмотря на то, что выглядят они отчасти идентично. Т.е. это еще один тип, который является наследником от `ItemsListLine<SaleItem>`, который в свою очередь является наследником для `ItemsListLine<T>`. Таким образом, в дальнейшем мы по сути создаем экземпляры класс `ItemsListLine` и мы можем не менять сигнатуры вызовов конструкторов для этого класса, которые были реализованы на прошлом занятии.

    ```csharp

        public class ItemsListLine : ItemsListLine<SaleItem>
        {
            public ItemsListLine(Product product, int requestedCount) : base(product, requestedCount) { }

            public ItemsListLine(Service service) : base(service, 1) { }
        }
    ```

5. Линия с торговой единицей теперь не должна отслеживать количество добавленных позиций в корзине. Поэтому нам необходимо переработать этот момент:

    В `SaleItem` реализуем виртуальное свойство для запрещающее добавлять больше одной позиции товарной единициы:

    ```csharp
        /// <summary>
        /// Флаг, обозначающий, что может быть только одна товарная единица 
        /// </summary>
        public virtual bool OnyOneItem => false;
    ```

    Делаем его перегрузку в `Service`:

    ```csharp
        /// <inheritdoc/>
        public override bool OnyOneItem => true;
    ```

    В `ItemsListLine` дорабатываем свойство `Count`.
    Теперь у нас линия корзины не отслеживает количество добавленных записей торговой единицы.
    За этим будет следить сама корзина.

    ```csharp
        /// <summary>
        /// Количество элементов в линии
        /// </summary>
        public int Count { get; set; }
    ```

6. Правим корзину на работу с обобщенным типом:

    Правим коллекцию с линиями и сигнатуры существующих методов проверки линий:
    
    ```csharp
        ...

        private readonly List<ItemsListLine<SaleItem>> _lines = new ();

        ...

        private bool IsLineExists(Product product, out ItemsListLine<SaleItem> line)

        ...

        private bool IsLineExists(Service service, out ItemsListLine<SaleItem> line)
    ```

    Добавляем новый метод проверки линий на использование обобщенного типа:

    ```csharp
        // Добавляем новый метод проверки существования линии в корзине для использования обобщенного типа
        private bool IsLineExists(SaleItem saleItem, out ItemsListLine<SaleItem> line)
        {
            var saleItemType = saleItem is Service ? ItemTypes.Service : ItemTypes.Product;

            foreach (var ln in _lines)
            {
                if (ln.ItemType == saleItemType && ln.ItemId == saleItem.Id)
                {
                    line = ln;
                    return true;
                }
            }

            line = null!;
            return false;
        }
    ```

    Правим метод добавления линии с услугой, чтобы он использовал `IsLineExists(SaleItem saleItem, out ItemsListLine<SaleItem> line)` 

    ```csharp
        /// <summary>
        /// Добавить услугу в корзину
        /// </summary>
        public string AddLine(Service service)
        {
            var saleItem = service as SaleItem;

            if (IsLineExists(saleItem, out _))
            {
                if (saleItem.OnyOneItem)
                    return $"Ошибка при добавлении услуги. Услуга \'{service.Name}\' уже добавлена в корзину";
            }

            _lines.Add(new ItemsListLine(service));
            return $"В корзину добавлена услуга \'{service.Name}\'";
        }
    ```

7. Правим заказ на работу с обобщенным типом:

    ```csharp
        ...

        private readonly List<ItemsListLine<SaleItem>> _lines;

        ...

        public Order(List<ItemsListLine<SaleItem>> lines)
    ```

8. Удаляем реализации методов `IsLineExists` под каждый свой тип.
    При этом видим, что проект у нас продолжаем компилироваться.
    В методе `AddLine(Service service)` У нас есть явное приведение типа `Service` к `SaleItem` поэтом здесь ничего не меняется, а вот в `AddLine(Product product, int requestedCount)` такого приведения нет и он начинает использовать следующую подходящую реализацию.

9. Убираем лишнее приведение типов в методе `AddLine(Service service)`

    ```csharp

        /// <summary>
        /// Добавить услугу в корзину
        /// </summary>
        public string AddLine(Service service)
        {
            if (IsLineExists(service, out _))
            {
                if (service.OnyOneItem)
                    return $"Ошибка при добавлении услуги. Услуга \'{service.Name}\' уже добавлена в корзину";
            }

            _lines.Add(new ItemsListLine(service));
            return $"В корзину добавлена услуга \'{service.Name}\'";
        }

    ```

10. В команде добавления элемента в корзину `AddBasketLineCommand.cs` используем полиморфизм. Удаляем ненужные методы `TryGetService`, `TryGetProduct`

    ```csharp
    private static bool TryGetItem<T>(int id, IEnumerable<T> items, out T item)
        where T: SaleItem
    {
        foreach (var saleItem in items)
        {
            if (saleItem.Id != id)
                continue;
            item = saleItem;
            return true;
        }

        item = null!;
        return false;
    }
    ```
