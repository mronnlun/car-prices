using System.Text.Json.Serialization;

namespace car_prices.Models;

public class BlocketSearchResponse
{
    [JsonPropertyName("docs")]
    public List<BlocketAd> Docs { get; set; } = [];

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class BlocketAd
{
    [JsonPropertyName("ad_id")]
    public long? AdId { get; set; }

    [JsonPropertyName("heading")]
    public string? Heading { get; set; }

    [JsonPropertyName("price")]
    public BlocketPrice? Price { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("canonical_url")]
    public string? CanonicalUrl { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("mileage")]
    public int? Mileage { get; set; }

    [JsonPropertyName("mileage_unit")]
    public string? MileageUnit { get; set; }

    [JsonPropertyName("make")]
    public string? Make { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("model_specification")]
    public string? ModelSpecification { get; set; }

    [JsonPropertyName("fuel")]
    public string? Fuel { get; set; }

    [JsonPropertyName("transmission")]
    public string? Transmission { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

public class BlocketPrice
{
    [JsonPropertyName("amount")]
    public int? Amount { get; set; }

    [JsonPropertyName("currency_code")]
    public string? CurrencyCode { get; set; }
}
