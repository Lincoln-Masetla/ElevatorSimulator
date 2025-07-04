using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Application.Common.Interfaces;

namespace ElevatorSimulator.Core.Application.Features.ElevatorManagement.Services;

/// <summary>
/// Service for managing elevator observers - implements Observer pattern in service layer
/// </summary>
public class ElevatorObserverService : IElevatorObserverService
{
    private readonly Dictionary<int, List<IElevatorObserver>> _observers = new();

    public void Subscribe(int elevatorId, IElevatorObserver observer)
    {
        if (!_observers.ContainsKey(elevatorId))
        {
            _observers[elevatorId] = new List<IElevatorObserver>();
        }

        if (!_observers[elevatorId].Contains(observer))
        {
            _observers[elevatorId].Add(observer);
        }
    }

    public void Unsubscribe(int elevatorId, IElevatorObserver observer)
    {
        if (_observers.ContainsKey(elevatorId))
        {
            _observers[elevatorId].Remove(observer);
        }
    }

    public void NotifyStateChanged(Elevator elevator)
    {
        if (_observers.ContainsKey(elevator.Id))
        {
            foreach (var observer in _observers[elevator.Id])
            {
                observer.OnElevatorStateChanged(elevator);
            }
        }
    }

    public void NotifyElevatorMoved(Elevator elevator, int previousFloor)
    {
        if (_observers.ContainsKey(elevator.Id))
        {
            foreach (var observer in _observers[elevator.Id])
            {
                observer.OnElevatorMoved(elevator, previousFloor);
            }
        }
    }

    public void NotifyPassengersChanged(Elevator elevator, int previousCount)
    {
        if (_observers.ContainsKey(elevator.Id))
        {
            foreach (var observer in _observers[elevator.Id])
            {
                observer.OnPassengersChanged(elevator, previousCount);
            }
        }
    }

    public void NotifyDestinationAdded(Elevator elevator, int floor)
    {
        if (_observers.ContainsKey(elevator.Id))
        {
            foreach (var observer in _observers[elevator.Id])
            {
                observer.OnDestinationAdded(elevator, floor);
            }
        }
    }

    public void NotifyDestinationReached(Elevator elevator, int floor)
    {
        if (_observers.ContainsKey(elevator.Id))
        {
            foreach (var observer in _observers[elevator.Id])
            {
                observer.OnDestinationReached(elevator, floor);
            }
        }
    }
}