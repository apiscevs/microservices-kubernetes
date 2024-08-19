using AutoMapper;
using CommandService.Models;
using CommandService.Settings;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using PlatformService;

namespace CommandService.SyncDataServices.Grpc;

public class PlatformDataClient : IPlatformDataClient
{
    private readonly IMapper _mapper;
    private readonly GrpcSettings _grpcSettings;

    public PlatformDataClient(IOptions<GrpcSettings> settings, IMapper mapper)
    {
        _mapper = mapper;
        _grpcSettings = settings.Value;
    }
    
    public async Task<ICollection<Platform>> GetAllPlatformsAsync()
    {
        Console.WriteLine($"Calling GRPC {_grpcSettings.GrpcPlatform}");
        
        var channel = GrpcChannel.ForAddress(_grpcSettings.GrpcPlatform);
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
            throw;
        }
    }
}