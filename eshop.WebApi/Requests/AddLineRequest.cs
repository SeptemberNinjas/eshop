using System.Text.Json.Serialization;

namespace eshop.WebApi.Requests;

public record AddLineRequest(
    [property: JsonPropertyName("id")]int ItemId,
    [property: JsonPropertyName("count")]int? CountToAdd);