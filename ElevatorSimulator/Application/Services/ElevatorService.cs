using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Domain.Exceptions;
using ElevatorSimulator.Domain.Interfaces;

namespace ElevatorSimulator.Application.Services;

/// <summary>
/// Service for elevator operations implementing clean architecture
/// </summary>
public class ElevatorService : IElevatorService
{
    private readonly IElevatorObserverService? _observerService;

    public ElevatorService(IElevatorObserverService? observerService = null)
    {
        _observerService = observerService;
    }
    public bool CanAcceptPassengers(Elevator elevator, int count)
    {
        return elevator.PassengerCount + count <= elevator.MaxCapacity;
    }

    public async Task LoadPassengersAsync(Elevator elevator, int count)
    {
        if (!CanAcceptPassengers(elevator, count))
            throw new CapacityExceededException($"Cannot load {count} passengers");

        elevator.State = ElevatorState.DoorsOpen;
        _observerService?.NotifyStateChanged(elevator);
        
        var previousCount = elevator.PassengerCount;
        elevator.PassengerCount += count;
        
        _observerService?.NotifyPassengersChanged(elevator, previousCount);
        
        elevator.State = ElevatorState.Idle;
        _observerService?.NotifyStateChanged(elevator);
    }

    public async Task UnloadPassengersAsync(Elevator elevator, int count)
    {
        if (count > elevator.PassengerCount)
            throw new CapacityExceededException($"Cannot unload {count} passengers. Current count: {elevator.PassengerCount}");

        elevator.State = ElevatorState.DoorsOpen;
        _observerService?.NotifyStateChanged(elevator);
        
        var previousCount = elevator.PassengerCount;
        elevator.PassengerCount -= count;
        
        _observerService?.NotifyPassengersChanged(elevator, previousCount);
        
        elevator.State = ElevatorState.Idle;
        _observerService?.NotifyStateChanged(elevator);
    }

    public void AddDestination(Elevator elevator, int floor)
    {
        if (floor < 1 || floor > 20)
            throw new InvalidFloorException($"Floor {floor} is invalid");

        if (!elevator.Destinations.Contains(floor) && floor != elevator.CurrentFloor)
        {
            elevator.Destinations.Add(floor);
            elevator.Destinations.Sort();
            
            _observerService?.NotifyDestinationAdded(elevator, floor);
        }
    }

    public async Task MoveToFloorAsync(Elevator elevator, int floor)
    {
        if (floor == elevator.CurrentFloor) return;

        var startFloor = elevator.CurrentFloor;
        elevator.State = ElevatorState.Moving;
        elevator.Direction = floor > elevator.CurrentFloor ? ElevatorDirection.Up : ElevatorDirection.Down;
        
        _observerService?.NotifyStateChanged(elevator);
        
        // Move elevator to destination floor
        var previousFloor = elevator.CurrentFloor;
        elevator.CurrentFloor = floor;
        _observerService?.NotifyElevatorMoved(elevator, previousFloor);
        
        if (elevator.Destinations.Contains(floor))
        {
            elevator.Destinations.Remove(floor);
            _observerService?.NotifyDestinationReached(elevator, floor);
        }

        if (!elevator.Destinations.Any())
        {
            elevator.Direction = ElevatorDirection.Idle;
            elevator.State = ElevatorState.Idle;
            _observerService?.NotifyStateChanged(elevator);
        }
    }

    public int? GetNextDestination(Elevator elevator)
    {
        if (!elevator.Destinations.Any()) return null;
        
        return elevator.Destinations
            .OrderBy(f => Math.Abs(f - elevator.CurrentFloor))
            .First();
    }

    public double CalculateDistance(Elevator elevator, int floor)
    {
        return Math.Abs(elevator.CurrentFloor - floor);
    }

    // Synchronous wrappers for testing compatibility
    public void LoadPassengers(Elevator elevator, int count)
    {
        LoadPassengersAsync(elevator, count).GetAwaiter().GetResult();
    }

    public void UnloadPassengers(Elevator elevator, int count)
    {
        UnloadPassengersAsync(elevator, count).GetAwaiter().GetResult();
    }

    public void MoveToFloor(Elevator elevator, int floor)
    {
        MoveToFloorAsync(elevator, floor).GetAwaiter().GetResult();
    }
}