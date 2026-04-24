using System.Net.Http.Json;
using System.Text.Json;
using car_prices.Models;
using Microsoft.Extensions.Logging;

namespace car_prices.Services;

public class NettiautoService(HttpClient httpClient, ILogger<NettiautoService> logger)
{
    private const string TokenUrl = "https://auth.nettix.fi/oauth2/token";
    private const string SearchUrl = "https://api.nettix.fi/rest/car/search?make=75&model=3630&priceFrom=15000&kilometersTo=80000&rows=100&sortBy=price&sortOrder=asc";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    public async Task<List<CarListing>> FetchListingsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Fetching listings from Nettiauto");

        var token = await GetAccessTokenAsync(ct);

        var request = new HttpRequestMessage(HttpMethod.Get, SearchUrl);
        request.Headers.Add("X-Access-Token", token);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var ads = await JsonSerializer.DeserializeAsync<List<NettiautoAd>>(
            await response.Content.ReadAsStreamAsync(ct), JsonOptions, ct);

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
            RowKey = ad.Id,
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

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        var clientId = Environment.GetEnvironmentVariable("NettiautoClientId")
            ?? throw new InvalidOperationException("NettiautoClientId not configured");
        var clientSecret = Environment.GetEnvironmentVariable("NettiautoClientSecret")
            ?? throw new InvalidOperationException("NettiautoClientSecret not configured");

        var content = new FormUrlEncodedContent([
            new("grant_type", "client_credentials"),
            new("client_id", clientId),
            new("client_secret", clientSecret)
        ]);

        var response = await httpClient.PostAsync(TokenUrl, content, ct);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return tokenResponse.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("No access_token in response");
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
