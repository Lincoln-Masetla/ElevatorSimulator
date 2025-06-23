namespace ElevatorSimulator.Domain.Exceptions;

public class InvalidFloorException : Exception
{
    public InvalidFloorException(string message) : base(message) { }
    
    public InvalidFloorException(string message, Exception innerException) : base(message, innerException) { }
}