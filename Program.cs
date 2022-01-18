using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RequestGatling;
using Serilog;


var host = Host
    .CreateDefaultBuilder()
    .ConfigureAppConfiguration(Bootstrap.LoadConfiguration)
    .ConfigureServices(Bootstrap.ConfigureServices)
    .ConfigureLogging(logBuilder =>
    {
        Log.Logger =  RequestGatling.Logging.LoggerFactory.CreateGlobalLogger();
        logBuilder.ClearProviders();
    })
    .UseSerilog()
    .Build();

try
{
    await host.RunAsync();
}
catch (Exception e)
{
    Log.Logger.Fatal(e, "Unexpected exception... shutting down!");
}