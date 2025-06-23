using ElevatorSimulator.Application.Services;
using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Interfaces;
using ElevatorSimulator.Presentation.Console;
using Moq;
using NUnit.Framework;
using System.Reflection;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class InputParsingTests
{
    private AppService _appService;
    private Mock<ISimulationService> _mockSimulation;

    [SetUp]
    public void SetUp()
    {
        _mockSimulation = new Mock<ISimulationService>();
        var mockUI = new Mock<ConsoleUI>();
        _appService = new AppService(_mockSimulation.Object, mockUI.Object);
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldHandleSimpleFormat()
    {
        // Arrange
        var input = "3 8 12, 7 2 8";

        // Act
        var result = CallParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].FromFloor, Is.EqualTo(3));
        Assert.That(result[0].ToFloor, Is.EqualTo(8));
        Assert.That(result[0].PassengerCount, Is.EqualTo(12));
        Assert.That(result[1].FromFloor, Is.EqualTo(7));
        Assert.That(result[1].ToFloor, Is.EqualTo(2));
        Assert.That(result[1].PassengerCount, Is.EqualTo(8));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldHandleSquareBrackets()
    {
        // Arrange
        var input = "[3 8 12, 7 2 8]";

        // Act
        var result = CallParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].FromFloor, Is.EqualTo(3));
        Assert.That(result[0].ToFloor, Is.EqualTo(8));
        Assert.That(result[0].PassengerCount, Is.EqualTo(12));
        Assert.That(result[1].FromFloor, Is.EqualTo(7));
        Assert.That(result[1].ToFloor, Is.EqualTo(2));
        Assert.That(result[1].PassengerCount, Is.EqualTo(8));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldHandlePartialBrackets()
    {
        // Arrange
        var input = "[3 8 12, 7 2 8";

        // Act
        var result = CallParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].FromFloor, Is.EqualTo(3));
        Assert.That(result[0].ToFloor, Is.EqualTo(8));
        Assert.That(result[0].PassengerCount, Is.EqualTo(12));
        Assert.That(result[1].FromFloor, Is.EqualTo(7));
        Assert.That(result[1].ToFloor, Is.EqualTo(2));
        Assert.That(result[1].PassengerCount, Is.EqualTo(8));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldHandleExtraSpaces()
    {
        // Arrange
        var input = "  3   8   12  ,   7   2   8  ";

        // Act
        var result = CallParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].FromFloor, Is.EqualTo(3));
        Assert.That(result[0].ToFloor, Is.EqualTo(8));
        Assert.That(result[0].PassengerCount, Is.EqualTo(12));
        Assert.That(result[1].FromFloor, Is.EqualTo(7));
        Assert.That(result[1].ToFloor, Is.EqualTo(2));
        Assert.That(result[1].PassengerCount, Is.EqualTo(8));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldHandleSingleTrip()
    {
        // Arrange
        var input = "5 10 15";

        // Act
        var result = CallParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].FromFloor, Is.EqualTo(5));
        Assert.That(result[0].ToFloor, Is.EqualTo(10));
        Assert.That(result[0].PassengerCount, Is.EqualTo(15));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldHandleMultipleTrips()
    {
        // Arrange
        var input = "1 5 8, 2 10 12, 3 15 20";

        // Act
        var result = CallParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].FromFloor, Is.EqualTo(1));
        Assert.That(result[0].ToFloor, Is.EqualTo(5));
        Assert.That(result[0].PassengerCount, Is.EqualTo(8));
        Assert.That(result[1].FromFloor, Is.EqualTo(2));
        Assert.That(result[1].ToFloor, Is.EqualTo(10));
        Assert.That(result[1].PassengerCount, Is.EqualTo(12));
        Assert.That(result[2].FromFloor, Is.EqualTo(3));
        Assert.That(result[2].ToFloor, Is.EqualTo(15));
        Assert.That(result[2].PassengerCount, Is.EqualTo(20));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldThrowException_WhenInvalidFormat()
    {
        // Arrange
        var input = "3 8, 7 2 8"; // Missing passenger count in first trip

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CallParseMultiDestinationInput(input));
        Assert.That(ex.Message, Contains.Substring("Invalid trip format"));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldThrowException_WhenInvalidFloorRange()
    {
        // Arrange
        var input = "25 8 12, 7 2 8"; // Floor 25 is invalid (max is 20)

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CallParseMultiDestinationInput(input));
        Assert.That(ex.Message, Contains.Substring("floor must be between 1 and 20"));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldThrowException_WhenSameFromAndToFloor()
    {
        // Arrange
        var input = "5 5 12, 7 2 8"; // Same from and to floor

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CallParseMultiDestinationInput(input));
        Assert.That(ex.Message, Contains.Substring("From floor and to floor must be different"));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldThrowException_WhenZeroPassengers()
    {
        // Arrange
        var input = "3 8 0, 7 2 8"; // Zero passengers

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => CallParseMultiDestinationInput(input));
        Assert.That(ex.Message, Contains.Substring("Number of passengers must be positive"));
    }

    // Helper method to call private ParseMultiDestinationInput method using reflection
    private List<Request> CallParseMultiDestinationInput(string input)
    {
        var method = typeof(AppService).GetMethod("ParseMultiDestinationInput", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(method, Is.Not.Null, "ParseMultiDestinationInput method not found");
        
        try
        {
            return (List<Request>)method.Invoke(_appService, new object[] { input });
        }
        catch (TargetInvocationException ex)
        {
            // Re-throw the inner exception to get the actual exception from the method
            throw ex.InnerException ?? ex;
        }
    }
}