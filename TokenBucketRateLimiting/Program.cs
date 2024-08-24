namespace TokenBucketRateLimiting;

public class TokenBucketRateLimiter
{
    private readonly int _bucketCapacity;
    private readonly int _tokensPerInterval;
    private readonly TimeSpan _interval;
    private int _tokens;
    private DateTime _lastRefill;

    public TokenBucketRateLimiter(int bucketCapacity, int tokensPerInterval, TimeSpan interval)
    {
        _bucketCapacity = bucketCapacity;
        _tokensPerInterval = tokensPerInterval;
        _interval = interval;
        _tokens = bucketCapacity;
        _lastRefill = DateTime.UtcNow;
    }

    public bool IsRequestAllowed()
    {
        RefillTokens();

        if (_tokens > 0)
        {
            _tokens--;
            return true;
        }

        return false;
    }

    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var elapsedTime = now - _lastRefill;

        if (elapsedTime >= _interval)
        {
            var tokensToAdd = (int)(elapsedTime.TotalMilliseconds / _interval.TotalMilliseconds) * _tokensPerInterval;
            _tokens = Math.Min(_bucketCapacity, _tokens + tokensToAdd);
            _lastRefill = now;
        }
    }
}

public class Program
{
    public static void Main()
    {
        var rateLimiter = new TokenBucketRateLimiter(5, 1, TimeSpan.FromSeconds(2)); // 5 tokens max, 1 token every 2 seconds

        for (int i = 0; i < 10; i++)
        {
            bool allowed = rateLimiter.IsRequestAllowed();
            Console.WriteLine($"Request {i + 1}: {(allowed ? "Allowed" : "Blocked")}");
            Thread.Sleep(1000); // Wait 1 second between requests
        }
    }
}