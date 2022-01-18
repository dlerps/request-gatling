using Serilog.Core;
using Serilog.Core.Enrichers;

namespace RequestGatling.Logging;

public static class LogEnrichers
{
    public static ILogEventEnricher AppEnricher =>
        new PropertyEnricher("Application", $"RequestGatling: {Bootstrap.Configuration["Request:RemoteAddress"]}");
}