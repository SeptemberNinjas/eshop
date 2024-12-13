namespace eshop.Core;

/// <summary>
/// Обобщенный интерфейс репозитория поддерживающего только чтение для взаимодействия с сущностями интернет-магазина
/// </summary>
public interface IReadOnlyRepository<T>
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
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}