using CommandService.Data;
using CommandService.Models;
using CommandService.SyncDataServices.Grpc;

namespace PlatformService.Data
{
    public static class PrepareDatabase
    {
        public static async Task PrepPopulationAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                var platforms = await grpcClient.GetAllPlatformsAsync();

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
                    repository.SaveChanges();
                }
            }
        }
    }
}

