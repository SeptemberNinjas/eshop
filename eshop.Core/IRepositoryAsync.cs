namespace eshop.Core;

/// <summary>
/// Обобщенный интерфейс репозитория для взаимодействия с сущностями интернет-магазина
/// </summary>
public interface IRepositoryAsync<T> : IReadOnlyRepositoryAsync<T>
{
    /// <summary>
    /// Обновить объект
    /// </summary>
    Task UpdateAsync(T item);

    /// <summary>
    /// Добавить новый объект
    /// </summary>
    /// <returns>Идентификатор созданного объекта</returns>
    Task<int> InsertAsync(T item);
}