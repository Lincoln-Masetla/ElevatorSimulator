using ElevatorSimulator.Core.Domain.Enums;

namespace ElevatorSimulator.Core.Domain.Entities;

public class Request
{
    public int FromFloor { get; set; }
    public int ToFloor { get; set; }
    public int PassengerCount { get; set; }
    public ElevatorDirection Direction => ToFloor > FromFloor ? ElevatorDirection.Up : ElevatorDirection.Down;

    public Request(int fromFloor, int toFloor, int passengerCount)
    {
        FromFloor = fromFloor;
        ToFloor = toFloor;
        PassengerCount = passengerCount;
    }

    // Backward compatibility
    public int Floor => ToFloor;
}