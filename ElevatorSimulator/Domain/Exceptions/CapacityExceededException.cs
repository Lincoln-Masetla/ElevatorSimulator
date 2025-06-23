namespace ElevatorSimulator.Domain.Exceptions;

public class CapacityExceededException : Exception
{
    public CapacityExceededException(string message) : base(message) { }
    
    public CapacityExceededException(string message, Exception innerException) : base(message, innerException) { }
}