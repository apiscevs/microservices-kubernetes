using System.Diagnostics.Metrics;

namespace PlatformService.Metrics;

public class ApiHitsMetrics
{
    private readonly Counter<long> _apiHitCounter;
    private readonly Counter<long> _dbQueryCounter;

    private readonly DateTime _startTime;
    
    public ApiHitsMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("PlatformService.Metrics");
        
        // Record the time the application started
        _startTime = DateTime.UtcNow;

        // Create an ObservableGauge to track uptime in seconds
        meter.CreateObservableGauge<long>("platformservice.uptime_seconds", () =>
        {
            Console.WriteLine("hit metrics");
            // Calculate uptime in seconds
            var uptime = (DateTime.UtcNow - _startTime).TotalSeconds;
            return (long)uptime;
        });
        
        // Create your custom counters
        _apiHitCounter = meter.CreateCounter<long>("platformservice.api_hits", "Count of API hits");
        
        _dbQueryCounter = meter.CreateCounter<long>("platformservice.db_queries", "Count of DB queries");
    }

    public void IncrementApiHits()
    {
        _apiHitCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "api_method_name"));
    }

    public void IncrementDbQueries()
    {
        _dbQueryCounter.Add(1, new KeyValuePair<string, object?>("query", "some_query_name"));
    }
}