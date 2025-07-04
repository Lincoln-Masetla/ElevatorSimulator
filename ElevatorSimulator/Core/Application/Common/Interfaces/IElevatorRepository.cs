using ElevatorSimulator.Core.Domain.Entities;

namespace ElevatorSimulator.Core.Application.Common.Interfaces;

/// <summary>
/// Repository for managing elevator instances
/// </summary>
public interface IElevatorRepository
{
    /// <summary>
    /// Gets all elevators in the building
    /// </summary>
    List<Elevator> GetAll();
    
    /// <summary>
    /// Gets elevator by ID
    /// </summary>
    Elevator? GetById(int id);
    
    /// <summary>
    /// Gets available elevators that can accept passengers
    /// </summary>
    List<Elevator> GetAvailable();
}