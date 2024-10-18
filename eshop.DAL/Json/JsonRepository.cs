using System.Text.Json;

namespace eshop.DAL.Json;

/// <summary>
/// Абстракция для выделения общей логики работы с Json
/// </summary>
internal abstract class JsonRepository<T>
{
    /// <summary>
    /// Путь к файлу
    /// </summary>
    private protected abstract string ResourceFilePath { get; }

    /// <summary>
    /// Получить список из файла
    /// </summary>
    private protected IEnumerable<T> GetItemsFromFile()
    {
        if (!File.Exists(ResourceFilePath))
        {
            using var sw = new StreamWriter(ResourceFilePath);
            sw.WriteLine("[]");
        }

        using var sr = new StreamReader(ResourceFilePath);

        var result = JsonSerializer.Deserialize<IEnumerable<T>>(sr.BaseStream);

        return (IReadOnlyCollection<T>)(result ?? []);
    }
    
    /// <summary>
    /// Перезаписать файл с новым списком
    /// </summary>
    private protected void SaveItemsToFile(IEnumerable<T> items)
    {
        if (!File.Exists(ResourceFilePath))
        {
            using var sw = new StreamWriter(ResourceFilePath);
            sw.WriteLine("[]");
        }
        
        File.WriteAllText(ResourceFilePath, JsonSerializer.Serialize(items));
    }
}