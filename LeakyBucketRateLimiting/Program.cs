namespace LeakyBucketRateLimiting;

public class LeakyBucketRateLimiter
{
    private readonly int _bucketCapacity;
    private readonly int _leakRate;
    private int _currentFill;
    private DateTime _lastLeakTime;

    public LeakyBucketRateLimiter(int bucketCapacity, int leakRate)
    {
        _bucketCapacity = bucketCapacity;
        _leakRate = leakRate;
        _currentFill = 0;
        _lastLeakTime = DateTime.UtcNow;
    }

    public bool IsRequestAllowed()
    {
        LeakBucket();

        if (_currentFill < _bucketCapacity)
        {
            _currentFill++;
            return true;
        }

        return false;
    }

    private void LeakBucket()
    {
        var now = DateTime.UtcNow;
        var elapsedTime = (int)(now - _lastLeakTime).TotalMilliseconds;

        if (elapsedTime >= _leakRate)
        {
            _currentFill = Math.Max(0, _currentFill - elapsedTime / _leakRate);
            _lastLeakTime = now;
        }
    }
}

public class Program
{
    public static void Main()
    {
        var rateLimiter = new LeakyBucketRateLimiter(5, 1000); // Bucket size 5, leaks 1 request per second

        for (int i = 0; i < 10; i++)
        {
            bool allowed = rateLimiter.IsRequestAllowed();
            Console.WriteLine($"Request {i + 1}: {(allowed ? "Allowed" : "Blocked")}");
            Thread.Sleep(500); // Wait 0.5 seconds between requests
        }
    }
}
