namespace ElevatorSimulator.Core.Domain.Entities;

/// <summary>
/// Thread-safe request queue for elevator requests
/// </summary>
public class RequestQueue
{
    private readonly Queue<Request> _pendingRequests = new();
    private readonly object _lock = new();

    public void Enqueue(Request request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        lock (_lock)
        {
            _pendingRequests.Enqueue(request);
        }
    }

    public Request? Dequeue()
    {
        lock (_lock)
        {
            return _pendingRequests.Count > 0 ? _pendingRequests.Dequeue() : null;
        }
    }

    public bool HasPendingRequests
    {
        get
        {
            lock (_lock)
            {
                return _pendingRequests.Count > 0;
            }
        }
    }

    public int PendingCount
    {
        get
        {
            lock (_lock)
            {
                return _pendingRequests.Count;
            }
        }
    }

    public Request? PeekNext()
    {
        lock (_lock)
        {
            return _pendingRequests.Count > 0 ? _pendingRequests.Peek() : null;
        }
    }
}