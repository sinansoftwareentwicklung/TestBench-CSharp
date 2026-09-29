using TestBench.DeviceServer;

var builder = WebApplication.CreateBuilder(args);

var arduinoSettings = new ArduinoSettings();
builder.Configuration.GetSection("ArduinoSettings").Bind(arduinoSettings);
builder.Services.AddSingleton(arduinoSettings);

var testConfig = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(
        "TestConfigurations/testSpecification.json",
        optional: false,
        reloadOnChange: false)
    .Build();

var testSpecification = new TestSpecification();
testConfig.Bind(testSpecification);

builder.Services.AddSingleton(testSpecification);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, o => o.Protocols =
        Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<DeviceServiceImpl>();
var app = builder.Build();

var database = new Database();
database.Initialize();

app.MapGrpcService<DeviceServiceImpl>();
app.MapGrpcReflectionService();

app.MapGet("/", () => "Device server is running. Use a gRPC client to connect.");

app.Run();