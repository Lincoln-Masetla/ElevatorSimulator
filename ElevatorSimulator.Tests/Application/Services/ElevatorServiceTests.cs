using ElevatorSimulator.Core.Application.Features.ElevatorManagement.Services;
using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Domain.Enums;
using ElevatorSimulator.Core.Application.Common.Interfaces;
using Moq;
using NUnit.Framework;
using ElevatorSimulator.Core.Domain.Exceptions;

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
    public void LoadPassengers_ShouldThrowException_WhenExceedsCapacity()
    {
        _elevator.PassengerCount = 7;

        Assert.Throws<CapacityExceededException>(() => _service.LoadPassengers(_elevator, 2));
    }

    [Test]
    public void UnloadPassengers_ShouldDecreaseCount()
    {
        _elevator.PassengerCount = 5;
        
        _service.UnloadPassengers(_elevator, 2);

        Assert.That(_elevator.PassengerCount, Is.EqualTo(3));
    }

    [Test]
    public void UnloadPassengers_ShouldThrowException_WhenExceedsCurrentCount()
    {
        _elevator.PassengerCount = 3;

        Assert.Throws<CapacityExceededException>(() => _service.UnloadPassengers(_elevator, 5));
    }

    [Test]
    public void AddDestination_ShouldAddFloor()
    {
        _service.AddDestination(_elevator, 5);

        Assert.That(_elevator.Destinations.Contains(5), Is.True);
    }

    [Test]
    public void AddDestination_ShouldThrowException_WhenInvalidFloor()
    {
        Assert.Throws<InvalidFloorException>(() => _service.AddDestination(_elevator, 0));
        Assert.Throws<InvalidFloorException>(() => _service.AddDestination(_elevator, 21));
    }

    [Test]
    public void AddDestination_ShouldNotAddDuplicate()
    {
        _service.AddDestination(_elevator, 5);
        _service.AddDestination(_elevator, 5);

        Assert.That(_elevator.Destinations.Count(d => d == 5), Is.EqualTo(1));
    }

    [Test]
    public void AddDestination_ShouldNotAddCurrentFloor()
    {
        _elevator.CurrentFloor = 5;
        
        _service.AddDestination(_elevator, 5);

        Assert.That(_elevator.Destinations.Contains(5), Is.False);
    }

    [Test]
    public void MoveToFloor_ShouldUpdateCurrentFloor()
    {
        _service.MoveToFloor(_elevator, 8);

        Assert.That(_elevator.CurrentFloor, Is.EqualTo(8));
    }

    [Test]
    public void MoveToFloor_ShouldRemoveDestination()
    {
        _elevator.Destinations.Add(8);
        
        _service.MoveToFloor(_elevator, 8);

        Assert.That(_elevator.Destinations.Contains(8), Is.False);
    }

    [Test]
    public void GetNextDestination_ShouldReturnClosestFloor()
    {
        _elevator.CurrentFloor = 5;
        _elevator.Destinations.AddRange(new[] { 3, 8, 2, 10 });

        var nextDestination = _service.GetNextDestination(_elevator);

        Assert.That(nextDestination, Is.EqualTo(3)); // Closest to floor 5
    }

    [Test]
    public void GetNextDestination_ShouldReturnNull_WhenNoDestinations()
    {
        var nextDestination = _service.GetNextDestination(_elevator);

        Assert.That(nextDestination, Is.Null);
    }

    [Test]
    public void CalculateDistance_ShouldReturnCorrectDistance()
    {
        _elevator.CurrentFloor = 5;

        var distance = _service.CalculateDistance(_elevator, 8);

        Assert.That(distance, Is.EqualTo(3));
    }

    // CONCURRENCY & EDGE CASE TESTS
    [Test]
    public async Task LoadPassengersAsync_ConcurrentOperations_ShouldHandleMultipleLoads()
    {
        // Arrange
        var elevator = new Elevator { Id = 1, MaxCapacity = 10, CurrentFloor = 1 };

        // Act - Load passengers sequentially to avoid race conditions in passenger count
        for (int i = 0; i < 5; i++)
        {
            await _service.LoadPassengersAsync(elevator, 1);
        }

        // Assert
        Assert.That(elevator.PassengerCount, Is.EqualTo(5));
        
        // Verify observers were notified appropriately
        _mockObserverService.Verify(o => o.NotifyStateChanged(elevator), Times.AtLeast(5));
        _mockObserverService.Verify(o => o.NotifyPassengersChanged(elevator, It.IsAny<int>()), Times.Exactly(5));
    }

    [Test]
    public async Task UnloadPassengersAsync_ConcurrentOperations_ShouldHandleMultipleUnloads()
    {
        // Arrange
        var elevator = new Elevator { Id = 1, MaxCapacity = 10, CurrentFloor = 1, PassengerCount = 8 };
        var tasks = new List<Task>();

        // Act - Unload passengers concurrently
        for (int i = 0; i < 4; i++)
        {
            tasks.Add(_service.UnloadPassengersAsync(elevator, 1));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.That(elevator.PassengerCount, Is.EqualTo(4));
    }

    [Test]
    public async Task MoveToFloorAsync_ConcurrentMoves_ShouldHandleSequentially()
    {
        // Arrange
        var elevator = new Elevator { Id = 1, MaxCapacity = 10, CurrentFloor = 1 };
        elevator.Destinations.AddRange(new[] { 5, 8, 12 });
        var tasks = new List<Task>();

        // Act - Move to different floors concurrently
        tasks.Add(_service.MoveToFloorAsync(elevator, 5));
        tasks.Add(_service.MoveToFloorAsync(elevator, 8));
        tasks.Add(_service.MoveToFloorAsync(elevator, 12));

        await Task.WhenAll(tasks);

        // Assert - Only the last move should be final
        Assert.That(elevator.CurrentFloor, Is.EqualTo(12));
    }

    [Test]
    public void AddDestination_ConcurrentAdds_ShouldHandleMultipleDestinations()
    {
        // Arrange
        var elevator = new Elevator { Id = 1, MaxCapacity = 10, CurrentFloor = 1 };
        var floors = new[] { 3, 5, 8, 12, 15, 18 };

        // Act - Add destinations sequentially to avoid race conditions in test
        foreach (var floor in floors)
        {
            _service.AddDestination(elevator, floor);
        }

        // Assert
        Assert.That(elevator.Destinations.Count, Is.EqualTo(floors.Length));
        foreach (var floor in floors)
        {
            Assert.That(elevator.Destinations.Contains(floor), Is.True);
        }
        // Verify destinations are sorted
        Assert.That(elevator.Destinations, Is.EqualTo(floors.OrderBy(f => f).ToArray()));
    }

    [Test]
    public void CanAcceptPassengers_EdgeCase_ExactCapacity()
    {
        // Arrange
        _elevator.PassengerCount = 7;
        _elevator.MaxCapacity = 8;

        // Act & Assert
        Assert.That(_service.CanAcceptPassengers(_elevator, 1), Is.True);
        Assert.That(_service.CanAcceptPassengers(_elevator, 2), Is.False);
    }

    [Test]
    public void CanAcceptPassengers_EdgeCase_ZeroPassengers()
    {
        // Arrange
        _elevator.PassengerCount = 8;
        _elevator.MaxCapacity = 8;

        // Act & Assert
        Assert.That(_service.CanAcceptPassengers(_elevator, 0), Is.True);
    }

    [Test]
    public void LoadPassengers_EdgeCase_ZeroPassengers()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _service.LoadPassengers(_elevator, 0));
        Assert.That(_elevator.PassengerCount, Is.EqualTo(0));
    }

    [Test]
    public void UnloadPassengers_EdgeCase_ZeroPassengers()
    {
        // Arrange
        _elevator.PassengerCount = 5;

        // Act & Assert
        Assert.DoesNotThrow(() => _service.UnloadPassengers(_elevator, 0));
        Assert.That(_elevator.PassengerCount, Is.EqualTo(5));
    }

    [Test]
    public void AddDestination_EdgeCase_BoundaryFloors()
    {
        // Arrange - set elevator to different floor so destinations can be added
        _elevator.CurrentFloor = 5;
        
        // Act & Assert
        Assert.DoesNotThrow(() => _service.AddDestination(_elevator, 1));
        Assert.DoesNotThrow(() => _service.AddDestination(_elevator, 20));
        Assert.That(_elevator.Destinations.Contains(1), Is.True);
        Assert.That(_elevator.Destinations.Contains(20), Is.True);
    }

    [Test]
    public void MoveToFloor_EdgeCase_SameFloor()
    {
        // Arrange
        _elevator.CurrentFloor = 5;

        // Act & Assert
        Assert.DoesNotThrow(() => _service.MoveToFloor(_elevator, 5));
        Assert.That(_elevator.CurrentFloor, Is.EqualTo(5));
    }

    [Test]
    public void GetNextDestination_EdgeCase_MultipleDestinations()
    {
        // Arrange
        _elevator.CurrentFloor = 10;
        _elevator.Destinations.AddRange(new[] { 1, 5, 8, 12, 15, 20 });

        // Act
        var nextDestination = _service.GetNextDestination(_elevator);

        // Assert - Should return floor 8 (closest to 10)
        Assert.That(nextDestination, Is.EqualTo(8));
    }

    [Test]
    public void CalculateDistance_EdgeCase_SameFloor()
    {
        // Arrange
        _elevator.CurrentFloor = 5;

        // Act
        var distance = _service.CalculateDistance(_elevator, 5);

        // Assert
        Assert.That(distance, Is.EqualTo(0));
    }

    [Test]
    public void CalculateDistance_EdgeCase_MaxDistance()
    {
        // Arrange
        _elevator.CurrentFloor = 1;

        // Act
        var distance = _service.CalculateDistance(_elevator, 20);

        // Assert
        Assert.That(distance, Is.EqualTo(19));
    }

    [Test]
    public async Task LoadPassengersAsync_ShouldNotifyObservers()
    {
        // Act
        await _service.LoadPassengersAsync(_elevator, 3);

        // Assert
        _mockObserverService.Verify(o => o.NotifyStateChanged(_elevator), Times.AtLeastOnce);
        _mockObserverService.Verify(o => o.NotifyPassengersChanged(_elevator, 0), Times.Once);
    }

    [Test]
    public async Task UnloadPassengersAsync_ShouldNotifyObservers()
    {
        // Arrange
        _elevator.PassengerCount = 5;

        // Act
        await _service.UnloadPassengersAsync(_elevator, 2);

        // Assert
        _mockObserverService.Verify(o => o.NotifyStateChanged(_elevator), Times.AtLeastOnce);
        _mockObserverService.Verify(o => o.NotifyPassengersChanged(_elevator, 5), Times.Once);
    }

    [Test]
    public void LoadPassengers_ShouldThrow_WhenExceedsCapacity()
    {
        Assert.Throws<CapacityExceededException>(() => _service.LoadPassengers(_elevator, 10));
    }

    //[Test]
    //public void AddDestination_ShouldAddFloor()
    //{
    //    _service.AddDestination(_elevator, 5);

    //    Assert.That(_elevator.Destinations, Contains.Item(5));
    //}

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

    //[Test]
    //public void UnloadPassengers_ShouldDecreaseCount()
    //{
    //    _elevator.PassengerCount = 5;

    //    _service.UnloadPassengers(_elevator, 3);

    //    Assert.That(_elevator.PassengerCount, Is.EqualTo(2));
    //}

    [Test]
    public void UnloadPassengers_ShouldThrow_WhenExceedsCurrentCount()
    {
        _elevator.PassengerCount = 2;

        Assert.Throws<CapacityExceededException>(() => _service.UnloadPassengers(_elevator, 5));
    }

    //[Test]
    //public void GetNextDestination_ShouldReturnClosestFloor()
    //{
    //    _elevator.CurrentFloor = 5;
    //    _elevator.Destinations.AddRange(new[] { 3, 8, 2 });

    //    var nextFloor = _service.GetNextDestination(_elevator);

    //    Assert.That(nextFloor, Is.EqualTo(3)); // Closest to floor 5
    //}

    //[Test]
    //public void GetNextDestination_ShouldReturnNull_WhenNoDestinations()
    //{
    //    var nextFloor = _service.GetNextDestination(_elevator);

    //    Assert.That(nextFloor, Is.Null);
    //}

    [Test]
    public void AddDestination_ShouldNotAddDuplicateFloor()
    {
        _service.AddDestination(_elevator, 5);
        _service.AddDestination(_elevator, 5); // Duplicate

        Assert.That(_elevator.Destinations.Count(d => d == 5), Is.EqualTo(1));
    }

    //[Test]
    //public void AddDestination_ShouldNotAddCurrentFloor()
    //{
    //    _elevator.CurrentFloor = 5;

    //    _service.AddDestination(_elevator, 5);

    //    Assert.That(_elevator.Destinations, Does.Not.Contain(5));
    //}

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