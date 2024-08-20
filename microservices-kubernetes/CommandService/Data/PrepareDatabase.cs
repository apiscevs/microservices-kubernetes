using CommandService.Data;
using CommandService.Models;
using CommandService.SyncDataServices.Grpc;
using Polly;

namespace PlatformService.Data
{
    public static class PrepareDatabase
    {
        public static async Task PrepPopulationAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();
                
                // CommandService can be initialized before Platform service is up.
                // Adding retry logic
                var retryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(2),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            Console.WriteLine($"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before next retry.");
                        });
                
                var platforms = await retryPolicy.ExecuteAsync(async () => 
                    await grpcClient.GetAllPlatformsAsync());

                await SeedDataAsync(serviceScope.ServiceProvider.GetService<ICommandRepository>(), platforms);
            }
        }
        
        private static async Task SeedDataAsync(ICommandRepository repository, ICollection<Platform> platforms)
        {
            Console.WriteLine("Seeding new platforms...");

            foreach (var plat in platforms)
            {
                var platformExists = await repository.PlatformExistsByExternalIdAsync(plat.ExternalId);

                if (!platformExists)
                {
                    await repository.CreatePlatformAsync(plat);
                }
            }
        }
    }
}