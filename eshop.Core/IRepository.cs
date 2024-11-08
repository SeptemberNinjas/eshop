namespace eshop.Core;

/// <summary>
/// Обобщенный интерфейс репозитория для взаимодействия с сущностями интернет-магазина
/// </summary>
public interface IRepository<T> : IReadOnlyRepository<T>
{
    /// <summary>
    /// Обновить объект
    /// </summary>
    Task UpdateAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новый объект
    /// </summary>
    /// <returns>Идентификатор созданного объекта</returns>
    Task<int> InsertAsync(T item, CancellationToken cancellationToken = default);
}