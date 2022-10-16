using System;
using System.Text;
using System.Text.Json;
using PlatformService.DTO;

namespace PlatformService.SyncDataServices.Http
{   
    public class CommandDataClient : ICommandDataClient
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;

        public CommandDataClient(HttpClient httpClient,
            IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
        }

        public async Task SendPlatformCommand(PlatformReadDTO platform)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(platform),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.
                PostAsync($"{configuration["CommandService"]}/api/c/Platforms", httpContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("--> Sync POST to command service was OK!");
            }
            else
            {
                Console.WriteLine("--> Sync POST to command service was NOT OK!");
            }
        }
    }
}

