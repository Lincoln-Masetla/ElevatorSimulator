using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Domain.Entities;

[TestFixture]
public class RequestTests
{
    [Test]
    public void Request_ShouldCalculateUpDirection()
    {
        // Arrange & Act
        var request = new Request(3, 10, 5);

        // Assert
        Assert.That(request.Direction, Is.EqualTo(ElevatorDirection.Up));
        Assert.That(request.FromFloor, Is.EqualTo(3));
        Assert.That(request.ToFloor, Is.EqualTo(10));
        Assert.That(request.PassengerCount, Is.EqualTo(5));
    }

    [Test]
    public void Request_ShouldCalculateDownDirection()
    {
        // Arrange & Act
        var request = new Request(15, 5, 8);

        // Assert
        Assert.That(request.Direction, Is.EqualTo(ElevatorDirection.Down));
        Assert.That(request.FromFloor, Is.EqualTo(15));
        Assert.That(request.ToFloor, Is.EqualTo(5));
        Assert.That(request.PassengerCount, Is.EqualTo(8));
    }

    [Test]
    public void Request_FloorProperty_ShouldReturnToFloor()
    {
        // Arrange & Act
        var request = new Request(3, 12, 4);

        // Assert
        Assert.That(request.Floor, Is.EqualTo(12)); // Backward compatibility
    }
}

[TestFixture]
public class MultiDestinationRequestTests
{
    [Test]
    public void MultiDestinationRequest_ShouldCreateWithDestinations()
    {
        // Arrange
        var destinations = new List<(int, int, int)>
        {
            (1, 5, 3),
            (2, 8, 4),
            (3, 12, 2)
        };

        // Act
        var multiRequest = new MultiDestinationRequest(destinations);

        // Assert
        Assert.That(multiRequest.Destinations.Count, Is.EqualTo(3));
        Assert.That(multiRequest.TotalPassengers, Is.EqualTo(9)); // 3 + 4 + 2
    }

    [Test]
    public void ToIndividualRequests_ShouldCreateSeparateRequests()
    {
        // Arrange
        var destinations = new List<(int, int, int)>
        {
            (1, 5, 3),
            (2, 8, 4)
        };
        var multiRequest = new MultiDestinationRequest(destinations);

        // Act
        var individualRequests = multiRequest.ToIndividualRequests();

        // Assert
        Assert.That(individualRequests.Count, Is.EqualTo(2));
        Assert.That(individualRequests[0].FromFloor, Is.EqualTo(1));
        Assert.That(individualRequests[0].ToFloor, Is.EqualTo(5));
        Assert.That(individualRequests[0].PassengerCount, Is.EqualTo(3));
        Assert.That(individualRequests[1].FromFloor, Is.EqualTo(2));
        Assert.That(individualRequests[1].ToFloor, Is.EqualTo(8));
        Assert.That(individualRequests[1].PassengerCount, Is.EqualTo(4));
    }

    [Test]
    public void TotalPassengers_ShouldSumAllPassengers()
    {
        // Arrange
        var destinations = new List<(int, int, int)>
        {
            (1, 5, 10),
            (2, 8, 15),
            (3, 12, 20)
        };
        var multiRequest = new MultiDestinationRequest(destinations);

        // Act & Assert
        Assert.That(multiRequest.TotalPassengers, Is.EqualTo(45)); // 10 + 15 + 20
    }

    [Test]
    public void MultiDestinationRequest_ShouldHandleEmptyDestinations()
    {
        // Arrange
        var destinations = new List<(int, int, int)>();
        var multiRequest = new MultiDestinationRequest(destinations);

        // Act & Assert
        Assert.That(multiRequest.Destinations.Count, Is.EqualTo(0));
        Assert.That(multiRequest.TotalPassengers, Is.EqualTo(0));
        Assert.That(multiRequest.ToIndividualRequests().Count, Is.EqualTo(0));
    }

    [Test]
    public void MultiDestinationRequest_ShouldHandleSingleDestination()
    {
        // Arrange
        var destinations = new List<(int, int, int)> { (5, 10, 8) };
        var multiRequest = new MultiDestinationRequest(destinations);

        // Act & Assert
        Assert.That(multiRequest.Destinations.Count, Is.EqualTo(1));
        Assert.That(multiRequest.TotalPassengers, Is.EqualTo(8));
        
        var individualRequests = multiRequest.ToIndividualRequests();
        Assert.That(individualRequests.Count, Is.EqualTo(1));
        Assert.That(individualRequests[0].FromFloor, Is.EqualTo(5));
        Assert.That(individualRequests[0].ToFloor, Is.EqualTo(10));
        Assert.That(individualRequests[0].PassengerCount, Is.EqualTo(8));
    }
}