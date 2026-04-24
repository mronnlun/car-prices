using Azure.Data.Tables;
using car_prices.Models;
using Microsoft.Extensions.Logging;

namespace car_prices.Services;

public class CarListingStore(TableServiceClient tableServiceClient, ILogger<CarListingStore> logger)
{
    private const string TableName = "CarListings";

    public async Task UpsertListingsAsync(List<CarListing> listings, CancellationToken ct = default)
    {
        var tableClient = tableServiceClient.GetTableClient(TableName);
        await tableClient.CreateIfNotExistsAsync(ct);

        var upserted = 0;
        var updated = 0;

        foreach (var listing in listings)
        {
            try
            {
                var existing = await GetExistingAsync(tableClient, listing.PartitionKey, listing.RowKey, ct);

                if (existing is not null)
                {
                    // Preserve FirstSeenAt, update LastSeenAt and other fields
                    listing.FirstSeenAt = existing.FirstSeenAt;
                    listing.LastSeenAt = DateTimeOffset.UtcNow;
                    updated++;
                }

                await tableClient.UpsertEntityAsync(listing, TableUpdateMode.Replace, ct);
                upserted++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upsert listing {Source}/{Id}", listing.PartitionKey, listing.RowKey);
            }
        }

        logger.LogInformation("Upserted {Upserted} listings ({Updated} updated) to {Table}",
            upserted, updated, TableName);
    }

    private static async Task<CarListing?> GetExistingAsync(
        TableClient tableClient, string partitionKey, string rowKey, CancellationToken ct)
    {
        try
        {
            var response = await tableClient.GetEntityAsync<CarListing>(partitionKey, rowKey, cancellationToken: ct);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}
