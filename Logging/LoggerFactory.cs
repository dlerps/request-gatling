using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RequestGatling.Logging;

public static class LoggerFactory
{
    public static Logger CreateGlobalLogger()
    {
        if (Bootstrap.Configuration == null)
            throw new InvalidOperationException("No configuration loaded... cannot configure logger");
        
        return new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Seq(
                Bootstrap.Configuration["Logging:Seq:IngestionEndpoint"], 
                apiKey: Bootstrap.Configuration["Logging:Seq:Key"])
            .Enrich.WithMachineName()
            .Enrich.FromLogContext()
            .Enrich.WithDemystifiedStackTraces()
            .Enrich.With(LogEnrichers.AppEnricher)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .CreateLogger();
    }
}