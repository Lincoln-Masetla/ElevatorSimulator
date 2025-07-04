using ElevatorSimulator.Core.Domain.Entities;

namespace ElevatorSimulator.Core.Application.Common.Interfaces;

public interface IElevatorObserver
{
    void OnElevatorStateChanged(Elevator elevator);
    void OnElevatorMoved(Elevator elevator, int previousFloor);
    void OnPassengersChanged(Elevator elevator, int previousCount);
    void OnDestinationAdded(Elevator elevator, int floor);
    void OnDestinationReached(Elevator elevator, int floor);
}