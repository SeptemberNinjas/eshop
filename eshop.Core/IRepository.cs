namespace eshop.Core
{
    /// <summary>
    /// Обобщенный интерфейс репозитория для взаимодействия с сущностями интернет-магазина
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Вернуть список всех элементов
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<T> GetAll();

        /// <summary>
        /// Получить количество элементов
        /// </summary>
        /// <returns></returns>
        int GetCount();

        /// <summary>
        /// Найти элемент по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns></returns>
        T? GetById(int id);
    }
}
