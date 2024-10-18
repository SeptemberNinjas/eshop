using eshop.Core;

namespace eshop.DAL.Json
{
    /// <summary>
    /// Реализация репозитория для хранения услуг в json'е
    /// </summary>
    internal class ServiceJsonReadOnlyRepository : JsonRepository<Service>, IReadOnlyRepository<Service>
    {
        private protected override string ResourceFilePath => "data\\services.json";
        
        /// <inheritdoc/>
        public IReadOnlyCollection<Service> GetAll() => (IReadOnlyCollection<Service>)GetItemsFromFile();

        /// <inheritdoc/>
        public Service? GetById(int id) => GetAll().FirstOrDefault(item => item.Id == id);

        /// <inheritdoc/>
        public int GetCount() => GetAll().Count;
    }
}
