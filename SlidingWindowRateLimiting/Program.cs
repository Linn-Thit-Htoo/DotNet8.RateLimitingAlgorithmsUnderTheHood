using System;
using System.Collections.Concurrent;

public class SlidingWindowRateLimiter
{
    private readonly int _limit;
    private readonly TimeSpan _windowSize;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _clients;

    public SlidingWindowRateLimiter(int limit, TimeSpan windowSize)
    {
        _limit = limit;
        _windowSize = windowSize;
        _clients = new ConcurrentDictionary<string, ConcurrentQueue<DateTime>>();
    }

    public bool IsRequestAllowed(string clientId)
    {
        var currentTime = DateTime.UtcNow;
        var requests = _clients.GetOrAdd(clientId, new ConcurrentQueue<DateTime>());

        // Clean up old requests outside the window
        while (requests.TryPeek(out var oldestRequest) && oldestRequest < currentTime - _windowSize)
        {
            requests.TryDequeue(out _);
        }

        if (requests.Count < _limit)
        {
            // Allow request and add the current request time
            requests.Enqueue(currentTime);
            return true;
        }

        // Request limit exceeded
        return false;
    }
}

public class Program
{
    public static void Main()
    {
        var rateLimiter = new SlidingWindowRateLimiter(5, TimeSpan.FromSeconds(10)); // 5 requests per 10 seconds
        string clientId = "client-1";

        for (int i = 0; i < 10; i++)
        {
            bool allowed = rateLimiter.IsRequestAllowed(clientId);
            Console.WriteLine($"Request {i + 1}: {(allowed ? "Allowed" : "Blocked")}");
            System.Threading.Thread.Sleep(1000); // Wait 1 second between requests
        }
    }
}
