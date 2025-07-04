using ElevatorSimulator.Core.Application.Common.Interfaces;
using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Application.Common.Models;

namespace ElevatorSimulator.Core.Application.Common.Validators;

/// <summary>
/// Consolidated validator for all elevator-related validations
/// </summary>
public class ElevatorRequestValidator : IValidator<ElevatorRequest>
{
    private readonly int _minFloor;
    private readonly int _maxFloor;

    public ElevatorRequestValidator(int minFloor = 1, int maxFloor = 20)
    {
        _minFloor = minFloor;
        _maxFloor = maxFloor;
    }

    public ValidationResult Validate(ElevatorRequest request)
    {
        var errors = new List<string>();

        if (request.Floor < _minFloor || request.Floor > _maxFloor)
        {
            errors.Add($"Floor must be between {_minFloor} and {_maxFloor}");
        }

        if (request.PassengerCount <= 0)
        {
            errors.Add("Passenger count must be greater than 0");
        }

        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a complete elevator trip request (from, to, passengers)
    /// </summary>
    public ValidationResult ValidateTripRequest(int fromFloor, int toFloor, int passengers)
    {
        var errors = new List<string>();

        if (fromFloor < _minFloor || fromFloor > _maxFloor)
        {
            errors.Add($"From floor must be between {_minFloor} and {_maxFloor}");
        }

        if (toFloor < _minFloor || toFloor > _maxFloor)
        {
            errors.Add($"To floor must be between {_minFloor} and {_maxFloor}");
        }

        if (fromFloor == toFloor)
        {
            errors.Add("From floor and to floor must be different");
        }

        if (passengers <= 0)
        {
            errors.Add("Number of passengers must be positive");
        }

        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates a Request object
    /// </summary>
    public ValidationResult ValidateRequest(Request request)
    {
        if (request == null)
        {
            return ValidationResult.Failure("Request cannot be null");
        }
        
        return ValidateTripRequest(request.FromFloor, request.ToFloor, request.PassengerCount);
    }
    
    /// <summary>
    /// Validates floor range only
    /// </summary>
    public ValidationResult ValidateFloor(int floor)
    {
        if (floor < _minFloor || floor > _maxFloor)
        {
            return ValidationResult.Failure($"Floor must be between {_minFloor} and {_maxFloor}");
        }
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates passenger count only
    /// </summary>
    public ValidationResult ValidatePassengerCount(int passengers)
    {
        if (passengers <= 0)
        {
            return ValidationResult.Failure("Number of passengers must be positive");
        }
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// Validates if an elevator can accept additional passengers
    /// </summary>
    public ValidationResult ValidateCapacity(int currentPassengers, int maxCapacity, int additionalPassengers)
    {
        if (currentPassengers + additionalPassengers > maxCapacity)
        {
            return ValidationResult.Failure($"Cannot add {additionalPassengers} passengers. Current: {currentPassengers}, Max: {maxCapacity}");
        }
        
        return ValidationResult.Success();
    }
}

