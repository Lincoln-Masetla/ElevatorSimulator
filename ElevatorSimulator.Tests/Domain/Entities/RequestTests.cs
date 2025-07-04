using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Domain.Enums;
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