namespace eshop.Core
{
    /// <summary>
    /// Абстрактная фабрика для создания репозиториев
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RepositoyFactory<T>
    {
        public abstract IRepository<T> Create();
    }
}
