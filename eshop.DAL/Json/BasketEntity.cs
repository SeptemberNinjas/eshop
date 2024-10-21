using eshop.Core;

namespace eshop.DAL.Json;

public class BasketEntity
{
    public IEnumerable<ItemsListLineEntity> Lines { get; init; }

    public static implicit operator Basket (BasketEntity entity)
    {
        return new Basket(entity.Lines
            .Select(l => (ItemsListLine)l));
    }
    
    public static implicit operator BasketEntity (Basket basket)
    {
        return new BasketEntity
        {
            Lines = basket.Lines.Select(l => (ItemsListLineEntity)l)
        };
    }
}
