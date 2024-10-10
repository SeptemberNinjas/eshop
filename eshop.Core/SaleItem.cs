namespace eshop.Core
{
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

        public SaleItem(int id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        /// <summary>
        /// Текстовое описание продажной единицы
        /// </summary>
        /// <returns></returns>
        public abstract string GetDisplayText();
    }
}
