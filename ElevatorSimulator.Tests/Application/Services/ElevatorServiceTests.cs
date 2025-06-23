using ElevatorSimulator.Application.Services;
using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Domain.Exceptions;
using ElevatorSimulator.Domain.Interfaces;
using Moq;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class ElevatorServiceTests
{
    private ElevatorService _service;
    private Mock<IElevatorObserverService> _mockObserverService;
    private Elevator _elevator;

    [SetUp]
    public void SetUp()
    {
        _mockObserverService = new Mock<IElevatorObserverService>();
        _service = new ElevatorService(_mockObserverService.Object);
        _elevator = new Elevator { Id = 1, MaxCapacity = 8, CurrentFloor = 1 };
    }

    [Test]
    public void CanAcceptPassengers_ShouldReturnTrue_WhenWithinCapacity()
    {
        _elevator.PassengerCount = 5;

        var result = _service.CanAcceptPassengers(_elevator, 2);

        Assert.That(result, Is.True);
    }

    [Test]
    public void CanAcceptPassengers_ShouldReturnFalse_WhenExceedsCapacity()
    {
        _elevator.PassengerCount = 7;

        var result = _service.CanAcceptPassengers(_elevator, 2);

        Assert.That(result, Is.False);
    }

    [Test]
    public void LoadPassengers_ShouldIncreaseCount()
    {
        _service.LoadPassengers(_elevator, 3);

        Assert.That(_elevator.PassengerCount, Is.EqualTo(3));
    }

    [Test]
    public void LoadPassengers_ShouldNotifyObservers()
    {
        // Act
        _service.LoadPassengers(_elevator, 3);

        // Assert
        _mockObserverService.Verify(o => o.NotifyPassengersChanged(_elevator, 0), Times.Once);
    }

    [Test]
    public void LoadPassengers_ShouldThrow_WhenExceedsCapacity()
    {
        Assert.Throws<CapacityExceededException>(() => _service.LoadPassengers(_elevator, 10));
    }

    [Test]
    public void AddDestination_ShouldAddFloor()
    {
        _service.AddDestination(_elevator, 5);

        Assert.That(_elevator.Destinations, Contains.Item(5));
    }

    [Test]
    public void AddDestination_ShouldNotifyObservers()
    {
        // Act
        _service.AddDestination(_elevator, 5);

        // Assert
        _mockObserverService.Verify(o => o.NotifyDestinationAdded(_elevator, 5), Times.Once);
    }

    [Test]
    public void AddDestination_ShouldThrow_WhenInvalidFloor()
    {
        Assert.Throws<InvalidFloorException>(() => _service.AddDestination(_elevator, 25));
    }

    [Test]
    public void MoveToFloor_ShouldUpdatePosition()
    {
        _elevator.Destinations.Add(3);
        
        _service.MoveToFloor(_elevator, 3);

        Assert.That(_elevator.CurrentFloor, Is.EqualTo(3));
        Assert.That(_elevator.Destinations, Does.Not.Contain(3));
    }

    [Test]
    public void CalculateDistance_ShouldReturnCorrectValue()
    {
        _elevator.CurrentFloor = 2;

        var distance = _service.CalculateDistance(_elevator, 6);

        Assert.That(distance, Is.EqualTo(4));
    }

    [Test]
    public void UnloadPassengers_ShouldDecreaseCount()
    {
        _elevator.PassengerCount = 5;

        _service.UnloadPassengers(_elevator, 3);

        Assert.That(_elevator.PassengerCount, Is.EqualTo(2));
    }

    [Test]
    public void UnloadPassengers_ShouldThrow_WhenExceedsCurrentCount()
    {
        _elevator.PassengerCount = 2;

        Assert.Throws<CapacityExceededException>(() => _service.UnloadPassengers(_elevator, 5));
    }

    [Test]
    public void GetNextDestination_ShouldReturnClosestFloor()
    {
        _elevator.CurrentFloor = 5;
        _elevator.Destinations.AddRange(new[] { 3, 8, 2 });

        var nextFloor = _service.GetNextDestination(_elevator);

        Assert.That(nextFloor, Is.EqualTo(3)); // Closest to floor 5
    }

    [Test]
    public void GetNextDestination_ShouldReturnNull_WhenNoDestinations()
    {
        var nextFloor = _service.GetNextDestination(_elevator);

        Assert.That(nextFloor, Is.Null);
    }

    [Test]
    public void AddDestination_ShouldNotAddDuplicateFloor()
    {
        _service.AddDestination(_elevator, 5);
        _service.AddDestination(_elevator, 5); // Duplicate

        Assert.That(_elevator.Destinations.Count(d => d == 5), Is.EqualTo(1));
    }

    [Test]
    public void AddDestination_ShouldNotAddCurrentFloor()
    {
        _elevator.CurrentFloor = 5;

        _service.AddDestination(_elevator, 5);

        Assert.That(_elevator.Destinations, Does.Not.Contain(5));
    }

    [Test]
    public void AddDestination_ShouldSortDestinations()
    {
        _service.AddDestination(_elevator, 10);
        _service.AddDestination(_elevator, 3);
        _service.AddDestination(_elevator, 7);

        Assert.That(_elevator.Destinations, Is.EqualTo(new[] { 3, 7, 10 }));
    }

    [Test]
    public void MoveToFloor_ShouldNotifyObservers_WhenMoving()
    {
        _elevator.CurrentFloor = 3;

        _service.MoveToFloor(_elevator, 8);

        _mockObserverService.Verify(o => o.NotifyStateChanged(_elevator), Times.AtLeast(1));
        _mockObserverService.Verify(o => o.NotifyElevatorMoved(_elevator, 3), Times.Once);
    }

    [Test]
    public void MoveToFloor_ShouldSetIdleState_WhenNoMoreDestinations()
    {
        _elevator.CurrentFloor = 3;
        _elevator.Destinations.Add(8);

        _service.MoveToFloor(_elevator, 8);

        Assert.That(_elevator.State, Is.EqualTo(ElevatorState.Idle));
        Assert.That(_elevator.Direction, Is.EqualTo(ElevatorDirection.Idle));
    }

    [Test]
    public void LoadPassengers_ShouldSetDoorsOpenState()
    {
        _service.LoadPassengers(_elevator, 3);

        // State changes: Idle -> DoorsOpen -> Idle
        _mockObserverService.Verify(o => o.NotifyStateChanged(_elevator), Times.Exactly(2));
    }

    [Test]
    public void CanAcceptPassengers_ShouldReturnTrue_WhenExactCapacity()
    {
        _elevator.PassengerCount = 5;

        var result = _service.CanAcceptPassengers(_elevator, 3); // Total = 8 (max capacity)

        Assert.That(result, Is.True);
    }

    [Test]
    public void CanAcceptPassengers_ShouldReturnFalse_WhenOverCapacity()
    {
        _elevator.PassengerCount = 6;

        var result = _service.CanAcceptPassengers(_elevator, 3); // Total = 9 (exceeds capacity)

        Assert.That(result, Is.False);
    }
}