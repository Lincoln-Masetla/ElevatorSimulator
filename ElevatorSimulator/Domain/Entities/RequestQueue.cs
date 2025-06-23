namespace ElevatorSimulator.Domain.Entities;

public class RequestQueue
{
    public Queue<Request> PendingRequests { get; private set; } = new();
    public DateTime LastProcessedTime { get; set; } = DateTime.Now;

    public void Enqueue(Request request)
    {
        PendingRequests.Enqueue(request);
    }

    public Request? Dequeue()
    {
        return PendingRequests.Count > 0 ? PendingRequests.Dequeue() : null;
    }

    public bool HasPendingRequests => PendingRequests.Count > 0;
    
    public int PendingCount => PendingRequests.Count;
    
    public Request? PeekNext()
    {
        return PendingRequests.Count > 0 ? PendingRequests.Peek() : null;
    }
}