using System.Data;
using eshop.Core;
using Npgsql;

namespace eshop.DAL.Database;

internal class OrderDatabaseRepository : DatabaseContext, IRepository<Order>
{
    public OrderDatabaseRepository(string connectionString) : base(connectionString)
    {
    }
    
    public IReadOnlyCollection<Order> GetAll()
    {
        using var command = GetCommand(
            $"""
             select o.*, ol.*, c.type, c.name, c.price, s.amount
                 from "order" o 
                 join order_line ol on o.id = ol.order_id
                 join catalog c on ol.item_id = c.id
                     left join stock s on c.id = s.id
             """);

        return GetOrders(command).ToArray();
    }

    public int GetCount()
    {
        using var command = GetCommand(
            $"""
             select count(*) from "order"
             """);

        var result = command.ExecuteScalar();

        return int.TryParse(result?.ToString(), out var count) ? count : 0;
    }

    public Order? GetById(int id)
    {
        using var command = GetCommand(
            $"""
             select o.*, ol.*, c.type, c.name, c.price, s.amount
                 from "order" o 
                 join order_line ol on o.id = ol.order_id
                 join catalog c on ol.item_id = c.id
                     left join stock s on c.id = s.id
                 where o.id = {id}
             """);

        return GetOrders(command).SingleOrDefault();
    }

    private static IEnumerable<Order> GetOrders(NpgsqlCommand command)
    {
        using var reader = command.ExecuteReader();
        if (!reader.HasRows)
            return [];

        var infos = new List<(ItemsListLine Line, int OrderId, OrderStatus OrderStatus)>();
        
        while (reader.Read())
        {
            infos.Add(GetOrderInfo(reader));
        }

        return infos.GroupBy(i => i.OrderId)
            .Select(g =>
                new Order(g.Key, g.First().OrderStatus, g.Select(gg => gg.Line)));
    }

    public void Update(Order item)
    {
        using var command = GetCommand(
            $"""
             delete from order_line where order_id = {item.Id};
             update "order" set status = {(int)item.Status} where id = {item.Id};
             insert into order_line(order_id, item_id, count) values 
             {string.Join(',', item.Lines.Select(l => $"({item.Id},{l.ItemId},{l.Count})"))}
             """);
        command.ExecuteNonQuery();
    }

    public int Insert(Order item)
    {
        using var command = GetCommand(
            $"""
             with inserted_id as (insert into "order"(status) values ({(int)item.Status}) returning id)
             insert into order_line(order_id, item_id, count) values 
             {string.Join(',', item.Lines.Select(l => $"((select id from inserted_id),{l.ItemId},{l.Count})"))}
             """);
        return command.ExecuteNonQuery();
    }

    private static (ItemsListLine Line, int OrderId, OrderStatus OrderStatus) GetOrderInfo(NpgsqlDataReader reader)
    {
        var itemType = (ItemTypes)reader.GetFieldValue<int>("type");
        SaleItem item = itemType == ItemTypes.Product
            ? new Product(reader.GetFieldValue<int>("item_id"),
                reader.GetFieldValue<string>("name"),
                reader.GetFieldValue<decimal>("price"),
                reader.GetFieldValue<int>("amount"))
            : new Service(reader.GetFieldValue<int>("item_id"),
                reader.GetFieldValue<string>("name"),
                reader.GetFieldValue<decimal>("price"));

        return (new ItemsListLine(item, reader.GetFieldValue<int>("count")), 
            reader.GetFieldValue<int>("id"), 
            (OrderStatus)reader.GetFieldValue<int>("status"));
    }
}