using ElevatorSimulator.Core.Application.Common.Models;

namespace ElevatorSimulator.Core.Application.Common.Interfaces;

public interface IValidator<T>
{
    ValidationResult Validate(T item);
}