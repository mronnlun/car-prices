using System.Text.Json;
using car_prices.Models;
using Microsoft.Extensions.Logging;

namespace car_prices.Services;

public class NettiautoService(HttpClient httpClient, ILogger<NettiautoService> logger)
{
    // Nettix REST API for saved search P649130595 (Volvo EX30)
    private const string SearchUrl = "https://www.nettiauto.com/hakutulokset?haku=P649130595";

    // Direct API endpoint as fallback — filters for Volvo EX30
    private const string ApiUrl = "https://api.nettix.fi/rest/car/search?make=75&model=3630&rows=100&sortBy=price&sortOrder=asc";

    public async Task<List<CarListing>> FetchListingsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Fetching listings from Nettiauto");

        var response = await httpClient.GetAsync(ApiUrl, ct);
        response.EnsureSuccessStatusCode();

        var ads = await JsonSerializer.DeserializeAsync<List<NettiautoAd>>(
            await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        if (ads is null)
        {
            logger.LogWarning("Nettiauto returned null response");
            return [];
        }

        logger.LogInformation("Nettiauto returned {Count} listings", ads.Count);

        var now = DateTimeOffset.UtcNow;
        return ads.Select(ad => new CarListing
        {
            PartitionKey = "nettiauto",
            RowKey = ad.Id.ToString(),
            Title = BuildTitle(ad),
            Price = ad.Price,
            Currency = ad.Currency ?? "EUR",
            Year = ad.Year,
            Mileage = ad.Kilometers,
            FuelType = ad.FuelType?.En ?? ad.FuelType?.Fi ?? "",
            GearType = ad.GearType?.En ?? ad.GearType?.Fi ?? "",
            Color = ad.Color?.En ?? ad.Color?.Fi ?? "",
            Location = BuildLocation(ad),
            Url = $"https://www.nettiauto.com/en/volvo/ex30/{ad.Id}",
            Status = ad.Status ?? "forsale",
            ListingDate = ad.DateCreated,
            FirstSeenAt = now,
            LastSeenAt = now
        }).ToList();
    }

    private static string BuildTitle(NettiautoAd ad)
    {
        var parts = new[] {
            ad.Make?.En ?? ad.Make?.Fi,
            ad.Model?.En ?? ad.Model?.Fi,
            ad.ModelType?.Name
        }.Where(p => !string.IsNullOrEmpty(p));
        return string.Join(" ", parts);
    }

    private static string BuildLocation(NettiautoAd ad)
    {
        var parts = new[] {
            ad.Town?.En ?? ad.Town?.Fi,
            ad.Region?.En ?? ad.Region?.Fi
        }.Where(p => !string.IsNullOrEmpty(p));
        return string.Join(", ", parts);
    }
}
