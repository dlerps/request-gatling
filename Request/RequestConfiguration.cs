using System.Collections.Generic;

namespace RequestGatling.Request;

public class RequestConfiguration
{
    public string? RemoteAddress { get; set; }

    public int? Timeout { get; set; }

    public IEnumerable<int>? AcceptedStatusCodes { get; set; }
}