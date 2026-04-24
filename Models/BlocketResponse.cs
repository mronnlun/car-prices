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
    public string? AdId { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("price")]
    public BlocketPrice? Price { get; set; }

    [JsonPropertyName("parameters")]
    public List<BlocketParameter> Parameters { get; set; } = [];

    [JsonPropertyName("share_url")]
    public string? ShareUrl { get; set; }

    [JsonPropertyName("location")]
    public List<BlocketLocation> Location { get; set; } = [];

    [JsonPropertyName("list_time")]
    public string? ListTime { get; set; }

    public string? GetParameter(string label)
    {
        return Parameters.FirstOrDefault(p =>
            string.Equals(p.Label, label, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}

public class BlocketPrice
{
    [JsonPropertyName("value")]
    public int? Value { get; set; }

    [JsonPropertyName("suffix")]
    public string? Suffix { get; set; }
}

public class BlocketParameter
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class BlocketLocation
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
