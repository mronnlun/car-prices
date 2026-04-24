using System.Text.Json;
using car_prices.Models;
using Microsoft.Extensions.Logging;

namespace car_prices.Services;

public class BlocketService(HttpClient httpClient, ILogger<BlocketService> logger)
{
    private const string SearchUrl =
        "https://www.blocket.se/mobility/search/api/search/SEARCH_ID_CAR_USED" +
        "?variant=1.818.2000653&sort=PRICE_ASC&price_from=160000&mileage_to=8000" +
        "&price_to=400000&rows=50";

    public async Task<List<CarListing>> FetchListingsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Fetching listings from Blocket");

        var request = new HttpRequestMessage(HttpMethod.Get, SearchUrl);
        request.Headers.Add("Accept", "application/json");

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var searchResult = await JsonSerializer.DeserializeAsync<BlocketSearchResponse>(
            await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        if (searchResult?.Docs is null)
        {
            logger.LogWarning("Blocket returned null response");
            return [];
        }

        logger.LogInformation("Blocket returned {Count} listings (total: {Total})",
            searchResult.Docs.Count, searchResult.Total);

        var now = DateTimeOffset.UtcNow;
        return searchResult.Docs
            .Where(ad => ad.AdId is not null)
            .Select(ad => new CarListing
            {
                PartitionKey = "blocket",
                RowKey = ad.AdId!.Value.ToString(),
                Title = BuildTitle(ad),
                Price = ad.Price?.Amount,
                Currency = ad.Price?.CurrencyCode ?? "SEK",
                Year = ad.Year,
                Mileage = ConvertMileage(ad.Mileage, ad.MileageUnit),
                FuelType = ad.Fuel ?? "",
                GearType = ad.Transmission ?? "",
                Color = "",
                Location = ad.Location ?? "",
                Url = ad.CanonicalUrl ?? "",
                Status = "forsale",
                ListingDate = ad.Timestamp is not null
                    ? DateTimeOffset.FromUnixTimeMilliseconds(ad.Timestamp.Value)
                    : null,
                FirstSeenAt = now,
                LastSeenAt = now
            }).ToList();
    }

    private static string BuildTitle(BlocketAd ad)
    {
        if (!string.IsNullOrEmpty(ad.ModelSpecification))
            return $"{ad.Make} {ad.Model} {ad.ModelSpecification}".Trim();
        return $"{ad.Make} {ad.Model}".Trim();
    }

    private static int? ConvertMileage(int? mileage, string? unit)
    {
        if (mileage is null) return null;
        // SCANDINAVIAN_MILE = 10 km
        return string.Equals(unit, "SCANDINAVIAN_MILE", StringComparison.OrdinalIgnoreCase)
            ? mileage.Value * 10
            : mileage.Value;
    }
}
