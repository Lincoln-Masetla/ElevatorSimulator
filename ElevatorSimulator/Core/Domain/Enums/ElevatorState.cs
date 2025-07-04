namespace ElevatorSimulator.Core.Domain.Enums;

public enum ElevatorState
{
    Idle,
    Moving,
    DoorsOpening,
    DoorsOpen,
    DoorsClosing,
    OutOfService
}