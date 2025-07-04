using ElevatorSimulator.Core.Domain.Entities;

namespace ElevatorSimulator.Core.Application.Common.Interfaces;

/// <summary>
/// Interface for elevator dispatching service
/// </summary>
public interface IDispatchService
{
    /// <summary>
    /// Finds the best elevator for a request
    /// </summary>
    Elevator? FindBestElevator(List<Elevator> elevators, Request request);
    
    /// <summary>
    /// Finds the closest elevator to a target floor
    /// </summary>
    Elevator? FindClosestElevator(List<Elevator> elevators, int targetFloor);
    
    /// <summary>
    /// Processes an elevator request by assigning and dispatching
    /// Returns the number of passengers that couldn't be accommodated
    /// </summary>
    Task<int> ProcessRequestAsync(List<Elevator> elevators, Request request);
}