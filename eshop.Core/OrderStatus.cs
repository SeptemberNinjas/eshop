namespace eshop.Core;

/// <summary>
/// Статус заказа
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Новый заказ
    /// </summary>
    New,
    
    /// <summary>
    /// Оплачен
    /// </summary>
    Paid,
    
    /// <summary>
    /// Завершен (выдан)
    /// </summary>
    Complete
}