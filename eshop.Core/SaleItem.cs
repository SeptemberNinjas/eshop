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
        public int Id { get; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; }

        public SaleItem(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Текстовое описание продажной единицы
        /// </summary>
        /// <returns></returns>
        public abstract string GetDisplayText();
    }
}
