using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Domain.Interfaces;

namespace ElevatorSimulator.Domain.Entities;

/// <summary>
/// Pure data entity representing an elevator with no business logic
/// </summary>
public class Elevator : IElevator
{
    public int Id { get; set; }
    public ElevatorType Type { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentFloor { get; set; } = 1;
    public ElevatorDirection Direction { get; set; } = ElevatorDirection.Idle;
    public ElevatorState State { get; set; } = ElevatorState.Idle;
    public int PassengerCount { get; set; } = 0;
    public List<int> Destinations { get; set; } = new();
}