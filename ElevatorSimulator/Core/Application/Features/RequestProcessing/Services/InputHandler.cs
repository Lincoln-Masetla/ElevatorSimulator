using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Application.Common.Validators;
using ElevatorSimulator.Core.Application.Common.Models;

namespace ElevatorSimulator.Core.Application.Features.RequestProcessing.Services;

/// <summary>
/// Handles user input parsing and validation - extracted from AppService
/// </summary>
public class InputHandler
{
    private readonly ElevatorRequestValidator _validator;

    public InputHandler()
    {
        _validator = new ElevatorRequestValidator();
    }
    public (int fromFloor, int toFloor, int passengers) GetElevatorRequestInput()
    {
        Console.WriteLine("\nELEVATOR REQUEST");
        Console.WriteLine("─────────────────────");
        
        // Get From Floor
        int fromFloor;
        while (true)
        {
            Console.Write("From Floor (1-20): ");
            var fromInput = Console.ReadLine()?.Trim();
            
            if (int.TryParse(fromInput, out fromFloor) && fromFloor >= 1 && fromFloor <= 20)
                break;
            
            if (string.IsNullOrEmpty(fromInput))
            {
                Console.WriteLine("Error: Please enter a floor number (you can't leave this blank)");
            }
            else if (!int.TryParse(fromInput, out _))
            {
                Console.WriteLine($"Error: '{fromInput}' is not a valid number. Please enter digits only (e.g., 5, 12)");
            }
            else
            {
                Console.WriteLine($"Error: Floor {fromFloor} is out of range. This building has floors 1-20 only.");
            }
        }
        
        // Get To Floor
        int toFloor;
        while (true)
        {
            Console.Write($"To Floor (1-20, current: {fromFloor}): ");
            var toInput = Console.ReadLine()?.Trim();
            
            if (int.TryParse(toInput, out toFloor) && toFloor >= 1 && toFloor <= 20 && toFloor != fromFloor)
                break;
            
            if (string.IsNullOrEmpty(toInput))
            {
                Console.WriteLine("Error: Please enter a destination floor (you can't leave this blank)");
            }
            else if (!int.TryParse(toInput, out _))
            {
                Console.WriteLine($"Error: '{toInput}' is not a valid number. Please enter digits only (e.g., 8, 15)");
            }
            else if (toFloor == fromFloor)
            {
                Console.WriteLine($"Error: You're already on floor {fromFloor}! Please choose a different destination.");
            }
            else if (toFloor < 1 || toFloor > 20)
            {
                Console.WriteLine($"Error: Floor {toFloor} doesn't exist. This building has floors 1-20 only.");
            }
        }
        
        // Get Number of Passengers
        int passengers;
        while (true)
        {
            Console.Write("Number of Passengers (max capacity varies by elevator): ");
            var passengersInput = Console.ReadLine()?.Trim();
            
            if (int.TryParse(passengersInput, out passengers) && passengers > 0)
                break;
            
            if (string.IsNullOrEmpty(passengersInput))
            {
                Console.WriteLine("Error: Please enter the number of passengers (you can't leave this blank)");
            }
            else if (!int.TryParse(passengersInput, out _))
            {
                Console.WriteLine($"Error: '{passengersInput}' is not a valid number. Please enter digits only (e.g., 3, 15)");
            }
            else if (passengers <= 0)
            {
                Console.WriteLine($"Error: You need at least 1 passenger to request an elevator (entered: {passengers})");
            }
            
            Console.WriteLine("TIP: Our elevators can handle 8-20 passengers depending on type");
        }
        
        return (fromFloor, toFloor, passengers);
    }

    public (int fromFloor, int toFloor, int passengers) ParseShorthandInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be empty");

        if (input.Contains(','))
            throw new ArgumentException("Multi-destination format detected. Use comma-separated format for multiple trips");
        
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length == 2 && int.TryParse(parts[0], out var toFloor) && int.TryParse(parts[1], out var passengers))
        {
            // Format: [to_floor] [passengers] (from floor 1)
            var validation = _validator.ValidateTripRequest(1, toFloor, passengers);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));
            return (1, toFloor, passengers);
        }
        else if (parts.Length == 3 && int.TryParse(parts[0], out var fromFloor) && 
                 int.TryParse(parts[1], out var toFloor2) && int.TryParse(parts[2], out var passengers2))
        {
            // Format: [from_floor] [to_floor] [passengers]
            var validation = _validator.ValidateTripRequest(fromFloor, toFloor2, passengers2);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));
            return (fromFloor, toFloor2, passengers2);
        }
        else if (parts.Length == 1 && int.TryParse(parts[0], out var toFloor3))
        {
            // Format: [to_floor] (1 passenger from floor 1)
            var validation = _validator.ValidateTripRequest(1, toFloor3, 1);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));
            return (1, toFloor3, 1);
        }
        else
        {
            throw new ArgumentException("Invalid format. Use: [to_floor] [passengers] or [from_floor] [to_floor] [passengers] or just [to_floor]");
        }
    }
    
    public List<Request> ParseMultiDestinationInput(string input)
    {
        var requests = new List<Request>();
        var tripStrings = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var tripString in tripStrings)
        {
            var cleanTripString = tripString.Trim();
            var parts = cleanTripString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 3 && 
                int.TryParse(parts[0], out var fromFloor) && 
                int.TryParse(parts[1], out var toFloor) && 
                int.TryParse(parts[2], out var passengers))
            {
                var validation = _validator.ValidateTripRequest(fromFloor, toFloor, passengers);
                if (!validation.IsValid)
                    throw new ArgumentException($"Invalid trip '{cleanTripString}': {string.Join(", ", validation.Errors)}");
                requests.Add(new Request(fromFloor, toFloor, passengers));
            }
            else
            {
                throw new ArgumentException($"Invalid trip format: '{cleanTripString}'. Each trip must be: [from_floor] [to_floor] [passengers]");
            }
        }
        
        return requests;
    }
}