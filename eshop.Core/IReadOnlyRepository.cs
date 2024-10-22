namespace eshop.Core;

/// <summary>
/// Обобщенный интерфейс репозитория поддерживающего только чтение для взаимодействия с сущностями интернет-магазина
/// </summary>
public interface IReadOnlyRepository<T>
{
    /// <summary>
    /// Вернуть список всех элементов
    /// </summary>
    IReadOnlyCollection<T> GetAll();

    /// <summary>
    /// Получить количество элементов
    /// </summary>
    int GetCount();

    /// <summary>
    /// Найти элемент по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор</param>
    T? GetById(int id);
}