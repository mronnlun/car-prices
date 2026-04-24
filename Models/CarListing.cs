using Azure;
using Azure.Data.Tables;

namespace car_prices.Models;

public class CarListing : ITableEntity
{
    /// <summary>
    /// PartitionKey = source (e.g. "nettiauto", "blocket")
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// RowKey = source-specific listing ID
    /// </summary>
    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Title { get; set; } = string.Empty;
    public int? Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int? Year { get; set; }
    public int? Mileage { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string GearType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ListingDate { get; set; }
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
}
