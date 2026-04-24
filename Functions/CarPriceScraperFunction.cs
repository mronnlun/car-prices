using car_prices.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace car_prices.Functions;

public class CarPriceScraperFunction(
    NettiautoService nettiautoService,
    BlocketService blocketService,
    CarListingStore store,
    ILogger<CarPriceScraperFunction> logger)
{
    [Function("CarPriceScraper")]
    public async Task Run(
        [TimerTrigger("0 0 */6 * * *", RunOnStartup = true)] TimerInfo timerInfo,
        CancellationToken ct)
    {
        logger.LogInformation("Car price scraper started at {Time}", DateTimeOffset.UtcNow);

        var nettiautoTask = FetchAndStoreAsync("Nettiauto", nettiautoService.FetchListingsAsync(ct), ct);
        var blocketTask = FetchAndStoreAsync("Blocket", blocketService.FetchListingsAsync(ct), ct);

        await Task.WhenAll(nettiautoTask, blocketTask);

        logger.LogInformation("Car price scraper completed. Next run: {Next}", timerInfo.ScheduleStatus?.Next);
    }

    private async Task FetchAndStoreAsync(string source, Task<List<Models.CarListing>> fetchTask, CancellationToken ct)
    {
        try
        {
            var listings = await fetchTask;
            await store.UpsertListingsAsync(listings, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch/store listings from {Source}", source);
        }
    }
}
