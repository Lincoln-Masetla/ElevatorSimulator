using ElevatorSimulator.Domain.Enums;

namespace ElevatorSimulator.Domain.Interfaces;

/// <summary>
/// Interface defining elevator entity contract
/// </summary>
public interface IElevator
{
    int Id { get; set; }
    ElevatorType Type { get; set; }
    int MaxCapacity { get; set; }
    int CurrentFloor { get; set; }
    ElevatorDirection Direction { get; set; }
    ElevatorState State { get; set; }
    int PassengerCount { get; set; }
    List<int> Destinations { get; set; }
}