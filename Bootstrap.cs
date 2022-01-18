using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RequestGatling.Request;

namespace RequestGatling;

public static class Bootstrap
{
    public static IConfiguration? Configuration { get; private set; }
    
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<RequestLoop>();
        services.Configure<RequestConfiguration>(Configuration!.GetSection("Request"));
    }

    public static void LoadConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile("appsettings.local.json", true, true)
            .AddEnvironmentVariables();

        Configuration = configurationBuilder.Build();
    }
}