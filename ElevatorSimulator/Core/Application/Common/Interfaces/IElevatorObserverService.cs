using ElevatorSimulator.Core.Domain.Entities;

namespace ElevatorSimulator.Core.Application.Common.Interfaces;

/// <summary>
/// Service interface for managing elevator observers
/// </summary>
public interface IElevatorObserverService
{
    /// <summary>
    /// Subscribes an observer to elevator events
    /// </summary>
    void Subscribe(int elevatorId, IElevatorObserver observer);
    
    /// <summary>
    /// Unsubscribes an observer from elevator events
    /// </summary>
    void Unsubscribe(int elevatorId, IElevatorObserver observer);
    
    /// <summary>
    /// Notifies observers of elevator state change
    /// </summary>
    void NotifyStateChanged(Elevator elevator);
    
    /// <summary>
    /// Notifies observers of elevator movement
    /// </summary>
    void NotifyElevatorMoved(Elevator elevator, int previousFloor);
    
    /// <summary>
    /// Notifies observers of passenger count change
    /// </summary>
    void NotifyPassengersChanged(Elevator elevator, int previousCount);
    
    /// <summary>
    /// Notifies observers of destination added
    /// </summary>
    void NotifyDestinationAdded(Elevator elevator, int floor);
    
    /// <summary>
    /// Notifies observers of destination reached
    /// </summary>
    void NotifyDestinationReached(Elevator elevator, int floor);
}