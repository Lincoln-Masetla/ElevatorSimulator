using ElevatorSimulator.Core.Domain.Entities;

namespace ElevatorSimulator.Core.Application.Common.Interfaces;

/// <summary>
/// Interface for elevator operations following DIP
/// </summary>
public interface IElevatorService
{
    /// <summary>
    /// Checks if elevator can accept additional passengers
    /// </summary>
    bool CanAcceptPassengers(Elevator elevator, int count);
    
    /// <summary>
    /// Loads passengers into elevator with realistic timing
    /// </summary>
    Task LoadPassengersAsync(Elevator elevator, int count);
    
    /// <summary>
    /// Unloads passengers from elevator with realistic timing
    /// </summary>
    Task UnloadPassengersAsync(Elevator elevator, int count);
    
    /// <summary>
    /// Adds destination to elevator queue
    /// </summary>
    void AddDestination(Elevator elevator, int floor);
    
    /// <summary>
    /// Moves elevator to specified floor with realistic movement simulation
    /// </summary>
    Task MoveToFloorAsync(Elevator elevator, int floor);
    
    /// <summary>
    /// Gets next destination for elevator
    /// </summary>
    int? GetNextDestination(Elevator elevator);
    
    /// <summary>
    /// Calculates distance between elevator and floor
    /// </summary>
    double CalculateDistance(Elevator elevator, int floor);
}