using System.Text.Json.Serialization;

namespace car_prices.Models;

public class NettiautoAd
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("make")]
    public NettiautoOption? Make { get; set; }

    [JsonPropertyName("model")]
    public NettiautoOption? Model { get; set; }

    [JsonPropertyName("modelType")]
    public NettiautoOptionWithName? ModelType { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("price")]
    public int? Price { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("kilometers")]
    public int? Kilometers { get; set; }

    [JsonPropertyName("fuelType")]
    public NettiautoOption? FuelType { get; set; }

    [JsonPropertyName("gearType")]
    public NettiautoOption? GearType { get; set; }

    [JsonPropertyName("color")]
    public NettiautoOption? Color { get; set; }

    [JsonPropertyName("region")]
    public NettiautoOption? Region { get; set; }

    [JsonPropertyName("town")]
    public NettiautoOption? Town { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("dateCreated")]
    public DateTimeOffset? DateCreated { get; set; }
}

public class NettiautoOption
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("fi")]
    public string? Fi { get; set; }

    [JsonPropertyName("en")]
    public string? En { get; set; }
}

public class NettiautoOptionWithName : NettiautoOption
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
