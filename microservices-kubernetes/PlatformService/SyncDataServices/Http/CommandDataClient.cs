using System.Text;
using System.Text.Json;
using PlatformService.DTO;

namespace PlatformService.SyncDataServices.Http
{   
    public class CommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _httpClient;

        public CommandDataClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendPlatformCommand(PlatformReadDto platform)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(platform),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.
                PostAsync("api/c/Platforms", httpContent);

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

