using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using yandex_search_mcp_dotnet.Models;
using yandex_search_mcp_dotnet.Services;
using yandex_search_mcp_dotnet.Tools;

var cliConfig = new ConfigurationBuilder()
    .AddCommandLine(args, new Dictionary<string, string>
    {
        ["--api-key"] = "ApiKey",
        ["--folder-id"] = "FolderId",
        ["--transport"] = "Transport",
        ["--host"] = "Host",
        ["--port"] = "Port",
    })
    .Build();

var apiKey = cliConfig["ApiKey"];
var folderId = cliConfig["FolderId"];
var transport = cliConfig["Transport"] ?? "stdio";
var host = cliConfig["Host"];
var rawPort = cliConfig["Port"];

if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(folderId))
{
    Console.Error.WriteLine("Error: --api-key and --folder-id are required.");
    Console.Error.WriteLine("Usage: dotnet run -- --api-key <key> --folder-id <id> [--transport <stdio|http>] [--host <host>] [--port <port>]");
    return 1;
}

if ((host is not null || rawPort is not null) && transport != "http")
{
    Console.Error.WriteLine("Error: --host and --port can only be used when --transport is 'http'.");
    return 1;
}

var config = new YandexConfig(apiKey, folderId);

if (transport == "http")
{
    int.TryParse(rawPort, out var port);
    var url = $"http://{host ?? "localhost"}:{(port > 0 ? port : 3001)}";

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("McpBrowser", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    builder.Services.AddSingleton(config);
    builder.Services.AddHttpClient<YandexSearchClient>();

    builder.Services.AddMcpServer()
        .WithHttpTransport(o => o.Stateless = true)
        .WithTools<WebSearchTool>();

    builder.Logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });

    var app = builder.Build();
    app.UseCors("McpBrowser");
    app.MapMcp().RequireCors("McpBrowser");
    app.Run(url);
    return 0;
}

var hostBuilder = Host.CreateApplicationBuilder(args);

hostBuilder.Services.AddSingleton(config);
hostBuilder.Services.AddHttpClient<YandexSearchClient>();

hostBuilder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<WebSearchTool>();

hostBuilder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

await hostBuilder.Build().RunAsync();
return 0;
