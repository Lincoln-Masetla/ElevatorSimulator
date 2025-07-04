using ElevatorSimulator.Core.Domain.Entities;

namespace ElevatorSimulator.Core.Application.Common.Interfaces;

/// <summary>
/// Service for managing elevator request queue
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// Adds a request to the queue
    /// </summary>
    void EnqueueRequest(Request request);
    
    /// <summary>
    /// Gets the next request from the queue
    /// </summary>
    Request? DequeueRequest();
    
    /// <summary>
    /// Peeks at the next request without removing it
    /// </summary>
    Request? PeekNextRequest();
    
    /// <summary>
    /// Checks if there are pending requests
    /// </summary>
    bool HasPendingRequests { get; }
    
    /// <summary>
    /// Gets the number of pending requests
    /// </summary>
    int PendingCount { get; }
    
    /// <summary>
    /// Gets the underlying queue for display purposes
    /// </summary>
    RequestQueue GetQueue();
}