using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotNetCommons.Web.Middleware;

/// Middleware for managing IP-based bans within an ASP.NET Core application.
/// This middleware tracks incoming requests, assigns a score to an IP based on request behavior,
/// and temporarily bans IPs that exceed defined thresholds.
public class IpBanMiddleware
{
    private class BanCounter
    {
        /// If set, the IP is banned until this time.
        public DateTime? BannedUntilZ { get; set; }

        /// When the last connection was seen, used as a back-off timer for the score and severity.
        public DateTime LastSeenZ { get; set; }

        /// This tracks how many failed requests we've seen; +1 per 401/403 response, and decreases with -1 per minute.
        public double Score { get; set; }

        /// This is a secondary score that tracks the severity, +1 for each ban that occurs; backs off with -1 per day.
        /// This causes the IP to be banned for longer periods as the severity increases.
        public double Severity { get; set; }
    }

    private readonly RequestDelegate _next;
    private readonly ILogger<IpBanMiddleware> _logger;
    private readonly TimeProvider _clock;
    private readonly ConcurrentDictionary<IPAddress, BanCounter> _ipBanCounter = [];
    private DateTime _lastSweep;

    public IpBanMiddleware(RequestDelegate next, ILogger<IpBanMiddleware> logger, TimeProvider clock)
    {
        _next   = next;
        _logger = logger;
        _clock  = clock;
    }

    public bool GetBanned(IPAddress ip)
    {
        return _ipBanCounter.TryGetValue(ip, out var counter) && DateTime.UtcNow < counter.BannedUntilZ;
    }

    public double GetScore(IPAddress ip)
    {
        return _ipBanCounter.TryGetValue(ip, out var counter) ? counter.Score : 0;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Fetch the IP address for this connection and lookup or create a new counter for this IP.
        // We're assuming that we run after the forwarded-address middleware.
        var ip  = context.Connection.RemoteIpAddress!;
        if (IPAddress.IsLoopback(ip))
        {
            _logger.LogDebug("IP request from loopback address, IP banning disabled");
            await _next(context);
            return;
        }

        var now  = _clock.UtcNow;
        var counter = _ipBanCounter.GetOrAdd(ip, _ => new BanCounter
        {
            // ReSharper disable once AccessToModifiedClosure
            LastSeenZ = now
        });

        // See if we're already banned
        if (now <= counter.BannedUntilZ)
        {
            _logger.LogDebug("IP {ip} is banned until {until}, blocking request", ip, counter.BannedUntilZ);
            await Respond(context, HttpStatusCode.TooManyRequests, $"{(int)HttpStatusCode.TooManyRequests} Too many failed requests");
            return;
        }

        // Back off the counters from the last seen time
        if (counter.Score > 0 || counter.Severity > 0)
        {
            lock (counter)
            {
                var elapsed = now - counter.LastSeenZ;

                counter.BannedUntilZ = null;
                counter.Score        = Math.Max(counter.Score - elapsed.TotalMinutes, 0);
                counter.Severity     = Math.Max(counter.Severity - elapsed.TotalDays, 0);
                counter.LastSeenZ    = now;
            }
        }

        // Perform the request
        await _next(context);

        // See if we need to sweep old, stale records
        now = _clock.UtcNow;
        if ((now - _lastSweep).TotalHours > 1)
        {
            _lastSweep = now;
            Sweep();
        }

        if (context.Response.StatusCode is not (401 or 403))
            return;

        // If we get a 401 or 403, we count this as a failed auth request. Increase the counters.
        lock (counter)
        {
            counter.Score++;
            if (counter.Score > 50)
            {
                // The score exceeded the threshold, ban the IP. Also increase the severity which guides us how long to ban this IP.
                counter.Severity++;
                counter.BannedUntilZ = now.AddMinutes(Math.Pow(10, counter.Severity));
                _logger.LogInformation("IP address {ip} has been banned until {until}", ip, counter.BannedUntilZ.Value.ToLocalTime());
            }
        }
    }

    /// Sends a response to the client with the specified HTTP status code and text content.
    private Task Respond(HttpContext context, HttpStatusCode status, string text)
    {
        context.Response.StatusCode    = (int)status;
        context.Response.ContentType   = "text/plain";
        context.Response.ContentLength = text.Length;
        return context.Response.WriteAsync(text);
    }

    /// Perform general maintenance on the IP ban records, clearing out old, stale records.
    private void Sweep()
    {
        var now      = _clock.UtcNow;
        var removals = _ipBanCounter
            .Where(x => x.Value.Score == 0 && (now - x.Value.LastSeenZ).TotalHours > 1)
            .Select(x => x.Key)
            .ToList();

        if (removals.Count == 0)
            return;

        // Remove old records not used in a while. Note: There is a slight possibility that this may interfere with
        // a current, ongoing request; however, this should not affect the system more than perhaps one missed auth request
        // and is deemed insignificant.
        foreach (var key in removals)
            _ipBanCounter.Remove(key, out _);

        _logger.LogDebug("Swept away {count} IP address ban records for house-cleaning", removals.Count);
    }
}
