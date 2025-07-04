using ElevatorSimulator.Core.Application.Features.RequestProcessing.Services;
using ElevatorSimulator.Core.Domain.Entities;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class InputHandlerTests
{
    private InputHandler _inputHandler;

    [SetUp]
    public void SetUp()
    {
        _inputHandler = new InputHandler();
    }

    [Test]
    public void ParseShorthandInput_ShouldParseThreeArguments()
    {
        // Arrange
        var input = "1 8 5";

        // Act
        var result = _inputHandler.ParseShorthandInput(input);

        // Assert
        Assert.That(result.fromFloor, Is.EqualTo(1));
        Assert.That(result.toFloor, Is.EqualTo(8));
        Assert.That(result.passengers, Is.EqualTo(5));
    }

    [Test]
    public void ParseShorthandInput_ShouldParseTwoArguments_WithFloorOne()
    {
        // Arrange
        var input = "8 5";

        // Act
        var result = _inputHandler.ParseShorthandInput(input);

        // Assert
        Assert.That(result.fromFloor, Is.EqualTo(1));
        Assert.That(result.toFloor, Is.EqualTo(8));
        Assert.That(result.passengers, Is.EqualTo(5));
    }

    [Test]
    public void ParseShorthandInput_ShouldParseOneArgument_WithDefaults()
    {
        // Arrange
        var input = "8";

        // Act
        var result = _inputHandler.ParseShorthandInput(input);

        // Assert
        Assert.That(result.fromFloor, Is.EqualTo(1));
        Assert.That(result.toFloor, Is.EqualTo(8));
        Assert.That(result.passengers, Is.EqualTo(1));
    }

    [Test]
    public void ParseShorthandInput_ShouldThrowException_WhenEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput(""));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput(null));
    }

    [Test]
    public void ParseShorthandInput_ShouldThrowException_WhenInvalidFormat()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("abc"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("1 abc 3"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("1 2 3 4 5"));
    }

    [Test]
    public void ParseShorthandInput_ShouldThrowException_WhenContainsComma()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("1 8 5, 3 12 2"));
    }

    [Test]
    public void ParseShorthandInput_ShouldValidateFloorRange()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("0 8 5"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("1 21 5"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("25 8 5"));
    }

    [Test]
    public void ParseShorthandInput_ShouldValidatePassengerCount()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("1 8 0"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("1 8 -5"));
    }

    [Test]
    public void ParseShorthandInput_ShouldValidateDifferentFloors()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseShorthandInput("5 5 3"));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldParseMultipleTrips()
    {
        // Arrange
        var input = "1 8 5, 3 12 2, 7 15 4";

        // Act
        var result = _inputHandler.ParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
        
        Assert.That(result[0].FromFloor, Is.EqualTo(1));
        Assert.That(result[0].ToFloor, Is.EqualTo(8));
        Assert.That(result[0].PassengerCount, Is.EqualTo(5));
        
        Assert.That(result[1].FromFloor, Is.EqualTo(3));
        Assert.That(result[1].ToFloor, Is.EqualTo(12));
        Assert.That(result[1].PassengerCount, Is.EqualTo(2));
        
        Assert.That(result[2].FromFloor, Is.EqualTo(7));
        Assert.That(result[2].ToFloor, Is.EqualTo(15));
        Assert.That(result[2].PassengerCount, Is.EqualTo(4));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldHandleSingleTrip()
    {
        // Arrange
        var input = "1 8 5";

        // Act
        var result = _inputHandler.ParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].FromFloor, Is.EqualTo(1));
        Assert.That(result[0].ToFloor, Is.EqualTo(8));
        Assert.That(result[0].PassengerCount, Is.EqualTo(5));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldHandleWhitespace()
    {
        // Arrange
        var input = " 1 8 5 , 3 12 2 ";

        // Act
        var result = _inputHandler.ParseMultiDestinationInput(input);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].FromFloor, Is.EqualTo(1));
        Assert.That(result[1].FromFloor, Is.EqualTo(3));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldThrowException_WhenInvalidTripFormat()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseMultiDestinationInput("1 8, 3 12 2"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseMultiDestinationInput("1 8 5, abc def ghi"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseMultiDestinationInput("1 8 5, "));
    }

    [Test]
    public void ParseMultiDestinationInput_ShouldValidateEachTrip()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseMultiDestinationInput("1 8 5, 0 12 2"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseMultiDestinationInput("1 8 5, 3 25 2"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseMultiDestinationInput("1 8 5, 3 3 2"));
        Assert.Throws<ArgumentException>(() => _inputHandler.ParseMultiDestinationInput("1 8 5, 3 12 0"));
    }
}