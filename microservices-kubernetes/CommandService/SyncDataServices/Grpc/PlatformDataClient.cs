using AutoMapper;
using CommandService.Models;
using Grpc.Net.Client;
using PlatformService;

namespace CommandService.SyncDataServices.Grpc;

public class PlatformDataClient : IPlatformDataClient
{
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public PlatformDataClient(IConfiguration configuration, IMapper mapper)
    {
        _configuration = configuration;
        _mapper = mapper;
    }
    
    public async Task<ICollection<Platform>> GetAllPlatformsAsync()
    {
        Console.WriteLine($"Calling GRPC {_configuration["GrpcPlatform"]}");

        Console.WriteLine($"Waiting 10 seconds...");
        // Hack to make sure platform service is up and running
        await Task.Delay(TimeSpan.FromSeconds(10));
        
        
        var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
        var client = new GrpcPlatform.GrpcPlatformClient(channel);
        var request = new GetAllRequest();

        try
        {
            var result = await client.GetAllPlatformsAsync(request);
            return _mapper.Map<ICollection<Platform>>(result.Platform);

        }
        catch (Exception e)
        {
            Console.WriteLine($"Error during Grpc call {e.Message}");
            return new List<Platform>();
        }
    }
}