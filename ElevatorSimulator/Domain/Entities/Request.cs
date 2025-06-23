using ElevatorSimulator.Domain.Enums;

namespace ElevatorSimulator.Domain.Entities;

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

public class MultiDestinationRequest
{
    public List<(int FromFloor, int ToFloor, int PassengerCount)> Destinations { get; set; }
    
    public MultiDestinationRequest(List<(int, int, int)> destinations)
    {
        Destinations = destinations;
    }
    
    public List<Request> ToIndividualRequests()
    {
        return Destinations.Select(d => new Request(d.FromFloor, d.ToFloor, d.PassengerCount)).ToList();
    }
    
    public int TotalPassengers => Destinations.Sum(d => d.PassengerCount);
}