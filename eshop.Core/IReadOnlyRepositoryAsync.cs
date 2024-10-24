namespace eshop.Core
{
    public interface IReadOnlyRepositoryAsync<T>
    {
        /// <summary>
        /// Вернуть список всех элементов
        /// </summary>
        Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить количество элементов
        /// </summary>
        Task<int> GetCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Найти элемент по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
