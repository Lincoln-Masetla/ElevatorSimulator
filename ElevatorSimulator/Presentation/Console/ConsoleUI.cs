using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Application.Observers;

namespace ElevatorSimulator.Presentation.Console;

public class ConsoleUI
{
    public void ShowElevators(List<Elevator> elevators, ElevatorLogger? logger = null)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== ELEVATOR STATUS (4 Elevators Available) ===");
        System.Console.WriteLine();
        
        foreach (var elevator in elevators)
        {
            var state = elevator.State switch
            {
                ElevatorState.Idle => "IDLE",
                ElevatorState.Moving => "MOVING",
                ElevatorState.DoorsOpen => "DOORS OPEN",
                _ => "UNKNOWN"
            };

            var direction = elevator.Direction switch
            {
                ElevatorDirection.Up => "UP",
                ElevatorDirection.Down => "DOWN",
                _ => "STOPPED"
            };

            var elevatorType = elevator.Type switch
            {
                ElevatorType.Standard => "Standard",
                ElevatorType.HighSpeed => "High-Speed",
                ElevatorType.Freight => "Freight",
                _ => "Unknown"
            };
            
            System.Console.WriteLine($"Elevator {elevator.Id} ({elevatorType}):");
            System.Console.WriteLine($"  Floor: {elevator.CurrentFloor}");
            System.Console.WriteLine($"  State: {state}");
            System.Console.WriteLine($"  Direction: {direction}");
            System.Console.WriteLine($"  Passengers: {elevator.PassengerCount}/{elevator.MaxCapacity}");
            
            if (elevator.Destinations.Any())
            {
                System.Console.WriteLine($"  Destinations: {string.Join(", ", elevator.Destinations)}");
            }
            System.Console.WriteLine();
        }
        
        System.Console.WriteLine("========================");
        
        // Show recent activity from logger
        if (logger != null)
        {
            var recentLogs = logger.GetRecentLogs(5);
            if (recentLogs.Any())
            {
                System.Console.WriteLine();
                System.Console.WriteLine("=== RECENT ACTIVITY ===");
                foreach (var log in recentLogs)
                {
                    System.Console.WriteLine(log);
                }
                System.Console.WriteLine("========================");
            }
        }
        
        System.Console.WriteLine();
    }

    public void ShowMessage(string message)
    {
        System.Console.WriteLine($"Message: {message}");
        System.Console.WriteLine("Press Enter to continue...");
        System.Console.ReadLine();
    }
}