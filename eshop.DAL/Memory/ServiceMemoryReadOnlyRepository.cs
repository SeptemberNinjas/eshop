using eshop.Core;

namespace eshop.DAL.Memory
{
    internal class ServiceMemoryReadOnlyRepository : IReadOnlyRepository<Service>
    {
        private readonly List<Service> _services;

        public ServiceMemoryReadOnlyRepository() {
            _services =
            [
                new Service(3, "Раскопать яму", 5.49m),
                new Service(4, "Вспахать поле", 1000)
            ];
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<Service> GetAll()
        {
            return _services.AsReadOnly();
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            return _services.Count;
        }

        /// <inheritdoc/>
        public Service? GetById(int id)
        {
            return _services.FirstOrDefault(item => item.Id == id);
        }
    }
}
