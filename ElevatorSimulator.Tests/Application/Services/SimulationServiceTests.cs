using ElevatorSimulator.Application.Services;
using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Domain.Interfaces;
using Moq;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class SimulationServiceTests
{
    private SimulationService _service;
    private Mock<IElevatorService> _mockElevatorService;
    private Mock<IDispatchService> _mockDispatchService;
    private Mock<IElevatorObserverService> _mockObserverService;

    [SetUp]
    public void SetUp()
    {
        _mockElevatorService = new Mock<IElevatorService>();
        _mockDispatchService = new Mock<IDispatchService>();
        _mockObserverService = new Mock<IElevatorObserverService>();
        
        _service = new SimulationService(
            _mockElevatorService.Object,
            _mockDispatchService.Object,
            _mockObserverService.Object);
    }

    [Test]
    public void GetElevators_ShouldReturn4Elevators()
    {
        // Act
        var elevators = _service.GetElevators();

        // Assert
        Assert.That(elevators.Count, Is.EqualTo(4));
        Assert.That(elevators.Count(e => e.Type == ElevatorType.Standard), Is.EqualTo(2));
        Assert.That(elevators.Count(e => e.Type == ElevatorType.HighSpeed), Is.EqualTo(1));
        Assert.That(elevators.Count(e => e.Type == ElevatorType.Freight), Is.EqualTo(1));
    }

    [Test]
    public void GetElevators_ShouldHaveCorrectCapacities()
    {
        // Act
        var elevators = _service.GetElevators();

        // Assert
        Assert.That(elevators.Where(e => e.Type == ElevatorType.Standard).All(e => e.MaxCapacity == 8), Is.True);
        Assert.That(elevators.Where(e => e.Type == ElevatorType.HighSpeed).All(e => e.MaxCapacity == 12), Is.True);
        Assert.That(elevators.Where(e => e.Type == ElevatorType.Freight).All(e => e.MaxCapacity == 20), Is.True);
    }

    [Test]
    public void HasAvailableElevator_ShouldReturnTrue_WhenElevatorAvailable()
    {
        // Arrange - All elevators start idle with 0 passengers

        // Act
        var hasAvailable = _service.HasAvailableElevator();

        // Assert
        Assert.That(hasAvailable, Is.True);
    }

    [Test]
    public void HasAvailableElevator_ShouldReturnFalse_WhenAllElevatorsAtCapacity()
    {
        // Arrange
        var elevators = _service.GetElevators();
        foreach (var elevator in elevators)
        {
            elevator.PassengerCount = elevator.MaxCapacity; // Fill to capacity
        }

        // Act
        var hasAvailable = _service.HasAvailableElevator();

        // Assert
        Assert.That(hasAvailable, Is.False);
    }

    [Test]
    public void HasAvailableElevator_ShouldReturnFalse_WhenAllElevatorsBusy()
    {
        // Arrange
        var elevators = _service.GetElevators();
        foreach (var elevator in elevators)
        {
            elevator.State = ElevatorState.Moving; // All busy
        }

        // Act
        var hasAvailable = _service.HasAvailableElevator();

        // Assert
        Assert.That(hasAvailable, Is.False);
    }

    [Test]
    public async Task ProcessRequestAsync_ShouldQueueRequest_WhenNoElevatorsAvailable()
    {
        // Arrange
        var elevators = _service.GetElevators();
        foreach (var elevator in elevators)
        {
            elevator.State = ElevatorState.Moving; // All busy
        }
        var request = new Request(1, 10, 5);

        // Act
        var remainingPassengers = await _service.ProcessRequestAsync(request);

        // Assert
        Assert.That(remainingPassengers, Is.EqualTo(5)); // All passengers queued
        Assert.That(_service.GetRequestQueue().HasPendingRequests, Is.True);
    }

    [Test]
    public async Task ProcessRequestAsync_ShouldCallDispatchService_WhenElevatorsAvailable()
    {
        // Arrange
        var request = new Request(1, 10, 5);
        _mockDispatchService.Setup(d => d.ProcessRequestAsync(It.IsAny<List<Elevator>>(), request))
            .ReturnsAsync(0); // All passengers accommodated

        // Act
        await _service.ProcessRequestAsync(request);

        // Assert
        _mockDispatchService.Verify(d => d.ProcessRequestAsync(It.IsAny<List<Elevator>>(), request), Times.Once);
    }

    [Test]
    public void GetClosestElevator_ShouldCallDispatchService()
    {
        // Arrange
        var targetFloor = 7;
        var expectedElevator = new Elevator { Id = 2 };
        _mockDispatchService.Setup(d => d.FindClosestElevator(It.IsAny<List<Elevator>>(), targetFloor))
            .Returns(expectedElevator);

        // Act
        var result = _service.GetClosestElevator(targetFloor);

        // Assert
        Assert.That(result, Is.EqualTo(expectedElevator));
        _mockDispatchService.Verify(d => d.FindClosestElevator(It.IsAny<List<Elevator>>(), targetFloor), Times.Once);
    }

    [Test]
    public void GetRequestQueue_ShouldReturnQueueInstance()
    {
        // Act
        var queue = _service.GetRequestQueue();

        // Assert
        Assert.That(queue, Is.Not.Null);
        Assert.That(queue.PendingCount, Is.EqualTo(0)); // Initially empty
    }

    [Test]
    public async Task ProcessQueuedRequestsAsync_ShouldProcessAllQueuedRequests()
    {
        // Arrange
        var queue = _service.GetRequestQueue();
        queue.Enqueue(new Request(1, 5, 3));
        queue.Enqueue(new Request(2, 8, 4));
        
        _mockDispatchService.Setup(d => d.ProcessRequestAsync(It.IsAny<List<Elevator>>(), It.IsAny<Request>()))
            .ReturnsAsync(0); // All passengers accommodated

        // Act
        await _service.ProcessQueuedRequestsAsync();

        // Assert
        Assert.That(queue.HasPendingRequests, Is.False); // Queue should be empty
        _mockDispatchService.Verify(d => d.ProcessRequestAsync(It.IsAny<List<Elevator>>(), It.IsAny<Request>()), Times.Exactly(2));
    }

    [Test]
    public async Task UpdateElevatorsAsync_ShouldMoveElevatorsToDestinations()
    {
        // Arrange
        var elevators = _service.GetElevators();
        elevators[0].Destinations.Add(5);
        elevators[1].Destinations.Add(10);
        
        _mockElevatorService.Setup(s => s.GetNextDestination(elevators[0])).Returns(5);
        _mockElevatorService.Setup(s => s.GetNextDestination(elevators[1])).Returns(10);
        _mockElevatorService.Setup(s => s.GetNextDestination(elevators[2])).Returns((int?)null);
        _mockElevatorService.Setup(s => s.GetNextDestination(elevators[3])).Returns((int?)null);

        // Act
        await _service.UpdateElevatorsAsync();

        // Assert
        _mockElevatorService.Verify(s => s.MoveToFloorAsync(elevators[0], 5), Times.Once);
        _mockElevatorService.Verify(s => s.MoveToFloorAsync(elevators[1], 10), Times.Once);
    }
}