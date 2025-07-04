namespace ElevatorSimulator.Core.Application.Common.Interfaces;

public interface IValidator<T>
{
    ValidationResult Validate(T item);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
}