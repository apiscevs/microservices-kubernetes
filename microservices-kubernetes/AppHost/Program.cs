using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: true);
var db = builder.AddSqlServer("sqlserver", password: sqlPassword, port: 45678)
    .WithDataVolume()
    .AddDatabase("PlatformsConnectionString", "PlatformService");

var platformService = builder.AddProject<PlatformService>("platformservice")
    .WithReference(db)
    .WithEnvironment("RabbitMqSettings__RabbitMqHost", "localhost")
    .WithEnvironment("RabbitMqSettings__RabbitMqPort", "5675");

var rabbitMqUser = builder.AddParameter("rabbitmq-username", secret: true);
var rabbitMqPassword = builder.AddParameter("rabbitmq-password", secret: true);
var messaging = builder.AddRabbitMQ("rabbitmq", port: 5675, userName: rabbitMqUser, password: rabbitMqPassword)
    .WithManagementPlugin()
    .WithDataVolume("rabbit-mq-volume-rly2")
    .WithEndpoint(name:"rabbit-mq-ui", port: 15675, targetPort: 15672, isProxied: true, scheme: "http");

var cache = builder.AddRedis("redis", port: 6689);

var commandService = builder.AddProject<CommandService>("commandservice")
    .WithReference(platformService)
    .WithReference(messaging)
    .WithReference(cache)
    .WithEnvironment("RabbitMqSettings__RabbitMqHost", "localhost")
    .WithEnvironment("RabbitMqSettings__RabbitMqPort", "5675")
    // Grpc
    .WithEnvironment("GrpcSettings__GrpcPlatform", "http://localhost:6666")
    // Redis (can be removed if proper connection string init will be used, then service discovery should kick-in);
    .WithEnvironment("RedisSettings__Endpoint", "localhost")
    .WithEnvironment("RedisSettings__Port", "6689");

platformService
    // Misc
    .WithEnvironment("CommandService", "http://commandservice") // service discovery should pick it
    .WithReference(commandService);

builder.Build().Run();