using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Application.Common.Interfaces;

namespace ElevatorSimulator.Core.Application.Features.RequestProcessing.Services;

/// <summary>
/// Service for managing elevator request queue - Single Responsibility: Queue management
/// </summary>
public class QueueService : IQueueService
{
    private readonly RequestQueue _requestQueue;

    public QueueService()
    {
        _requestQueue = new RequestQueue();
    }

    public void EnqueueRequest(Request request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        _requestQueue.Enqueue(request);
    }

    public Request? DequeueRequest() => _requestQueue.Dequeue();

    public Request? PeekNextRequest() => _requestQueue.PeekNext();

    public bool HasPendingRequests => _requestQueue.HasPendingRequests;

    public int PendingCount => _requestQueue.PendingCount;

    public RequestQueue GetQueue() => _requestQueue;
}