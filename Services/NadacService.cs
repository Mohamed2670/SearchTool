using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class NadacService(NadacRepository _nadacRepository)
    {
        public async Task GetMatchingPricesAsync(string csvUrl)
        {
            var items = await _nadacRepository.GetMatchingPricesAsync(csvUrl);
            Console.WriteLine($"Found {items.Count} matching NADAC records.");
            await _nadacRepository.AddNadacRecordsAsync(items);
            Console.WriteLine($"Processed {items.Count} NADAC records.");
            
        }

    }

}