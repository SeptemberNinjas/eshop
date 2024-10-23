using eshop.Core;

namespace eshop.DAL.Json;

public class ItemsListLineEntity
{
    public ItemTypes ItemType { get; set; }
    public Product? Product { get; set; }
    public Service? Service { get; set; }
    public int Count { get; set; }
    
    public static implicit operator ItemsListLine (ItemsListLineEntity entity)
    {
        return entity.ItemType == ItemTypes.Product
            ? new ItemsListLine(entity.Product!, entity.Count)
            : new ItemsListLine(entity.Service!);
    }
    
    public static implicit operator ItemsListLineEntity (ItemsListLine line)
    {
        return new ItemsListLineEntity
        {
            ItemType = line.ItemType,
            Product = line.ItemType is ItemTypes.Product ? line.SaleItem as Product : null,
            Service = line.ItemType is ItemTypes.Service ? line.SaleItem as Service : null,
            Count = line.Count
        };
    }
}
