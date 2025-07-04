using ElevatorSimulator.Core.Application.Common.Validators;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Validators;

[TestFixture]
public class ElevatorRequestValidatorTests
{
    private ElevatorRequestValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new ElevatorRequestValidator();
    }

    [Test]
    public void Validate_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var request = new ElevatorRequest(5, 3);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Validate_ShouldReturnFailure_WhenFloorTooLow()
    {
        // Arrange
        var request = new ElevatorRequest(0, 5);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Contains.Item("Floor must be between 1 and 20"));
    }

    [Test]
    public void Validate_ShouldReturnFailure_WhenFloorTooHigh()
    {
        // Arrange
        var request = new ElevatorRequest(25, 5);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Contains.Item("Floor must be between 1 and 20"));
    }

    [Test]
    public void Validate_ShouldReturnFailure_WhenPassengerCountZero()
    {
        // Arrange
        var request = new ElevatorRequest(5, 0);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Contains.Item("Passenger count must be greater than 0"));
    }

    [Test]
    public void Validate_ShouldReturnFailure_WhenPassengerCountNegative()
    {
        // Arrange
        var request = new ElevatorRequest(5, -3);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Contains.Item("Passenger count must be greater than 0"));
    }

    [Test]
    public void Validate_ShouldReturnMultipleErrors_WhenMultipleValidationFailures()
    {
        // Arrange
        var request = new ElevatorRequest(25, -5);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Contains.Item("Floor must be between 1 and 20"));
        Assert.That(result.Errors, Contains.Item("Passenger count must be greater than 0"));
    }

    [Test]
    public void Validate_ShouldReturnSuccess_WhenCustomLimits()
    {
        // Arrange
        var customValidator = new ElevatorRequestValidator(minFloor: 5, maxFloor: 15);
        var request = new ElevatorRequest(10, 3);

        // Act
        var result = customValidator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_ShouldReturnFailure_WhenCustomLimitsExceeded()
    {
        // Arrange
        var customValidator = new ElevatorRequestValidator(minFloor: 5, maxFloor: 15);
        var request = new ElevatorRequest(20, 3);

        // Act
        var result = customValidator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Contains.Item("Floor must be between 5 and 15"));
    }

    [Test]
    public void Validate_ShouldAllowLargePassengerCounts()
    {
        // Arrange - Test that passenger limit was removed
        var request = new ElevatorRequest(10, 100);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.That(result.IsValid, Is.True); // Should allow large passenger counts for queue system
    }

    [Test]
    public void ElevatorRequest_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var request = new ElevatorRequest(12, 25);

        // Assert
        Assert.That(request.Floor, Is.EqualTo(12));
        Assert.That(request.PassengerCount, Is.EqualTo(25));
    }
}