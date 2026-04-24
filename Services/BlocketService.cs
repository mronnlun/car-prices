using System.Text.Json;
using car_prices.Models;
using Microsoft.Extensions.Logging;

namespace car_prices.Services;

public class BlocketService(HttpClient httpClient, ILogger<BlocketService> logger)
{
    private const string SearchUrl =
        "https://www.blocket.se/mobility/search/api/search/SEARCH_ID_CAR_USED" +
        "?variant=1.818.2000653&sort=PRICE_ASC&price_from=160000&mileage_to=8000" +
        "&price_to=350000&rows=50";

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
                RowKey = ad.AdId!,
                Title = ad.Subject ?? "",
                Price = ad.Price?.Value,
                Currency = "SEK",
                Year = ParseInt(ad.GetParameter("Modellår") ?? ad.GetParameter("year")),
                Mileage = ParseMileage(ad.GetParameter("Miltal") ?? ad.GetParameter("mileage")),
                FuelType = ad.GetParameter("Drivmedel") ?? ad.GetParameter("fuel") ?? "",
                GearType = ad.GetParameter("Växellåda") ?? ad.GetParameter("gearbox") ?? "",
                Color = ad.GetParameter("Färg") ?? ad.GetParameter("color") ?? "",
                Location = string.Join(", ", ad.Location.Select(l => l.Name).Where(n => n is not null)),
                Url = ad.ShareUrl ?? "",
                Status = "forsale",
                ListingDate = ParseDate(ad.ListTime),
                FirstSeenAt = now,
                LastSeenAt = now
            }).ToList();
    }

    private static int? ParseInt(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var result) ? result : null;
    }

    private static int? ParseMileage(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        // Blocket formats mileage like "1 234 mil" or "1234" — convert mil to km
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (!int.TryParse(digits, out var result)) return null;
        // If value contains "mil" it's Swedish miles (1 mil = 10 km)
        return value.Contains("mil", StringComparison.OrdinalIgnoreCase) ? result * 10 : result;
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        return DateTimeOffset.TryParse(value, out var result) ? result : null;
    }
}
