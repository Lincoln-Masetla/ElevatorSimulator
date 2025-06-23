using ElevatorSimulator.Domain.Entities;

namespace ElevatorSimulator.Domain.Interfaces;

/// <summary>
/// Interface for simulation management following DIP
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// Gets all elevators in the system
    /// </summary>
    List<Elevator> GetElevators();
    
    /// <summary>
    /// Processes an elevator request with realistic simulation
    /// Returns the number of passengers that couldn't be accommodated
    /// </summary>
    Task<int> ProcessRequestAsync(Request request);
    
    /// <summary>
    /// Gets closest available elevator to target floor
    /// </summary>
    Elevator? GetClosestElevator(int targetFloor);
    
    /// <summary>
    /// Updates elevator states
    /// </summary>
    void UpdateElevators();
    
    /// <summary>
    /// Gets the request queue for monitoring pending requests
    /// </summary>
    RequestQueue GetRequestQueue();
    
    /// <summary>
    /// Checks if any elevator is available
    /// </summary>
    bool HasAvailableElevator();
    
    /// <summary>
    /// Processes queued requests when elevators become available
    /// </summary>
    Task ProcessQueuedRequestsAsync();
}