using Npgsql;
using System.Data.Common;

namespace eshop.DAL.Database
{
    public abstract class BaseRepository
    {
        private readonly DatabaseContext _databaseContext;

        public BaseRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public NpgsqlCommand GetCommand(string text)
        {
            return Task.Run(async () => await _databaseContext.GetCommandAsync(text)).Result;
        }

        public async Task<List<T>> ExecuteReaderListAsync<T>(string commandText, Func<DbDataReader, T> binging, CancellationToken cancellationToken)
        {
            var command = await _databaseContext.GetCommandAsync(commandText);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var result = new List<T>();

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(binging(reader));
            }

            return result;
        }

        public async Task<T?> ExecuteReaderAsync<T>(string commandText, Func<DbDataReader, T> binding, CancellationToken cancellationToken)
        {
            var command = await _databaseContext.GetCommandAsync(commandText);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                return binding(reader);


            return default;
        }

        public async Task<string?> ExecuteScalarAsync(string commandText, CancellationToken cancellationToken)
        {
            var command = await _databaseContext.GetCommandAsync(commandText);

            var reader = await command.ExecuteScalarAsync(cancellationToken);
            return reader?.ToString();
        }
    }
}
