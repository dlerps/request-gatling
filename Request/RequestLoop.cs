using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RequestGatling.Request;

public class RequestLoop : BackgroundService
{
    private const int InvalidStatusCode = 0;
    
    private readonly ILogger<RequestLoop> _logger;

    private readonly string _remoteAddress;

    private readonly int _timeout;

    private readonly IEnumerable<int> _acceptedStatusCodes;

    public RequestLoop(IOptions<RequestConfiguration> requestConfigurationOptions, ILogger<RequestLoop> logger)
    {
        var requestConfiguration = requestConfigurationOptions.Value;

        if (String.IsNullOrEmpty(requestConfiguration.RemoteAddress))
            throw new InvalidOperationException("Missing remote address");
        
        _remoteAddress = requestConfiguration.RemoteAddress;
        _timeout = requestConfiguration.Timeout ?? 10000;
        _acceptedStatusCodes = requestConfiguration.AcceptedStatusCodes ?? new[] { 200 } as IEnumerable<int>;
        
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            do
            {
                await DoRequest(stoppingToken);
                await Task.Delay(_timeout, stoppingToken);
            } while (true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Shutting down...");
        }
    }

    private async Task DoRequest(CancellationToken cancellationToken)
    {
        var rolex = Stopwatch.StartNew();

        try
        {
            var response = await _remoteAddress.GetAsync(cancellationToken);

            rolex.Stop();

            LogResponse(response, rolex.ElapsedMilliseconds);
        }
        catch (FlurlHttpException httpException)
        {
            LogFailedRequest(httpException.StatusCode ?? InvalidStatusCode, rolex.ElapsedMilliseconds, httpException);
        }
        catch (Exception exception)
        {
            LogFailedRequest(InvalidStatusCode, rolex.ElapsedMilliseconds, exception);
        }
    }

    private void LogResponse(IFlurlResponse response, long elapsedMs)
    {
        if (_acceptedStatusCodes.Contains(response.StatusCode))
        {
            _logger.LogInformation(
                "Request to {RemoteAddress} was successful ({StatusCode}) in {RequestDuration} ms",
                _remoteAddress,
                response.StatusCode,
                elapsedMs
            );

            return;
        }
        
        LogFailedRequest(response.StatusCode, elapsedMs);
    }

    private void LogFailedRequest(int statusCode, long elapsedMs, Exception exception = null)
    {
        const string msg = "Request to {RemoteAddress} failed ({StatusCode}) in {RequestDuration} ms";
        
        if (exception == null)
            _logger.LogWarning(msg, _remoteAddress, statusCode, elapsedMs);
        else
            _logger.LogError(exception, msg, _remoteAddress, statusCode, elapsedMs);
    }
}