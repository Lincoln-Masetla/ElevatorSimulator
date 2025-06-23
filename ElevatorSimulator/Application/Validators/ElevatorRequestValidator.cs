using ElevatorSimulator.Domain.Interfaces;

namespace ElevatorSimulator.Application.Validators;

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
}

public record ElevatorRequest(int Floor, int PassengerCount);