namespace eshop.Core
{
    public static class ItemTypesExtensions
    {
        public static string GetDisplayText(this ItemTypes itemType)
        {
            return itemType switch
            {
                ItemTypes.Product => "Товар",
                ItemTypes.Service => "Услуга",
                _ => ""
            };
        }
    }
}
