using ElevatorSimulator.Core.Application.Features.SimulationControl.Services;
using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Domain.Enums;
using ElevatorSimulator.Core.Application.Common.Interfaces;
using ElevatorSimulator.Core.Application.Common.Observers;
using Moq;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class SimulationServiceTests
{
    private SimulationService _service;
    private Mock<IElevatorRepository> _mockElevatorRepository;
    private Mock<IElevatorService> _mockElevatorService;
    private Mock<IDispatchService> _mockDispatchService;
    private Mock<IElevatorObserverService> _mockObserverService;
    private Mock<IQueueService> _mockQueueService;
    private List<Elevator> _testElevators;

    [SetUp]
    public void SetUp()
    {
        _mockElevatorRepository = new Mock<IElevatorRepository>();
        _mockElevatorService = new Mock<IElevatorService>();
        _mockDispatchService = new Mock<IDispatchService>();
        _mockObserverService = new Mock<IElevatorObserverService>();
        _mockQueueService = new Mock<IQueueService>();

        // Create test elevators
        _testElevators = new List<Elevator>
        {
            new() { Id = 1, Type = ElevatorType.Standard, MaxCapacity = 8, State = ElevatorState.Idle },
            new() { Id = 2, Type = ElevatorType.Standard, MaxCapacity = 8, State = ElevatorState.Idle },
            new() { Id = 3, Type = ElevatorType.HighSpeed, MaxCapacity = 12, State = ElevatorState.Idle },
            new() { Id = 4, Type = ElevatorType.Freight, MaxCapacity = 20, State = ElevatorState.Idle }
        };

        _mockElevatorRepository.Setup(r => r.GetAll()).Returns(_testElevators);
        _mockElevatorRepository.Setup(r => r.GetAvailable()).Returns(_testElevators);
        _mockQueueService.Setup(q => q.GetQueue()).Returns(new RequestQueue());

        _service = new SimulationService(
            _mockElevatorRepository.Object,
            _mockElevatorService.Object,
            _mockDispatchService.Object,
            _mockObserverService.Object,
            _mockQueueService.Object);
    }

    [Test]
    public void GetElevators_ShouldReturnElevatorsFromRepository()
    {
        // Act
        var elevators = _service.GetElevators();

        // Assert
        Assert.That(elevators.Count, Is.EqualTo(4));
        // Note: GetAll() is called once in constructor for observer subscription 
        // and once more in GetElevators(), so total should be twice
        _mockElevatorRepository.Verify(r => r.GetAll(), Times.Exactly(2));
    }

    [Test]
    public void GetLogger_ShouldReturnElevatorLogger()
    {
        // Act
        var logger = _service.GetLogger();

        // Assert
        Assert.That(logger, Is.Not.Null);
        Assert.That(logger, Is.TypeOf<ElevatorLogger>());
    }

    [Test]
    public async Task ProcessRequestAsync_ShouldQueueRequest_WhenNoElevatorsAvailable()
    {
        // Arrange
        _mockElevatorRepository.Setup(r => r.GetAvailable()).Returns(new List<Elevator>());
        var request = new Request(1, 10, 5);

        // Act
        var remainingPassengers = await _service.ProcessRequestAsync(request);

        // Assert
        Assert.That(remainingPassengers, Is.EqualTo(5)); // All passengers queued
        _mockQueueService.Verify(q => q.EnqueueRequest(request), Times.Once);
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
        _mockDispatchService.Verify(d => d.ProcessRequestAsync(_testElevators, request), Times.Once);
    }

    [Test]
    public async Task ProcessRequestAsync_ShouldEnqueueRemainingPassengers()
    {
        // Arrange
        var request = new Request(1, 10, 10);
        _mockDispatchService.Setup(d => d.ProcessRequestAsync(It.IsAny<List<Elevator>>(), request))
            .ReturnsAsync(3); // 7 passengers accommodated, 3 remaining

        // Act
        var remainingPassengers = await _service.ProcessRequestAsync(request);

        // Assert
        Assert.That(remainingPassengers, Is.EqualTo(3));
        _mockQueueService.Verify(q => q.EnqueueRequest(It.Is<Request>(r => r.PassengerCount == 3)), Times.Once);
    }

    [Test]
    public void GetClosestElevator_ShouldCallDispatchService()
    {
        // Arrange
        var targetFloor = 7;
        var expectedElevator = _testElevators[1];
        _mockDispatchService.Setup(d => d.FindClosestElevator(_testElevators, targetFloor))
            .Returns(expectedElevator);

        // Act
        var result = _service.GetClosestElevator(targetFloor);

        // Assert
        Assert.That(result, Is.EqualTo(expectedElevator));
        _mockDispatchService.Verify(d => d.FindClosestElevator(_testElevators, targetFloor), Times.Once);
    }

    [Test]
    public void HasAvailableElevator_ShouldReturnTrue_WhenElevatorsAvailable()
    {
        // Arrange
        _mockElevatorRepository.Setup(r => r.GetAvailable()).Returns(_testElevators);

        // Act
        var hasAvailable = _service.HasAvailableElevator();

        // Assert
        Assert.That(hasAvailable, Is.True);
    }

    [Test]
    public void HasAvailableElevator_ShouldReturnFalse_WhenNoElevatorsAvailable()
    {
        // Arrange
        _mockElevatorRepository.Setup(r => r.GetAvailable()).Returns(new List<Elevator>());

        // Act
        var hasAvailable = _service.HasAvailableElevator();

        // Assert
        Assert.That(hasAvailable, Is.False);
    }

    [Test]
    public void GetRequestQueue_ShouldReturnQueueFromService()
    {
        // Arrange
        var expectedQueue = new RequestQueue();
        _mockQueueService.Setup(q => q.GetQueue()).Returns(expectedQueue);

        // Act
        var queue = _service.GetRequestQueue();

        // Assert
        Assert.That(queue, Is.EqualTo(expectedQueue));
        _mockQueueService.Verify(q => q.GetQueue(), Times.Once);
    }

    [Test]
    public async Task UpdateElevatorsAsync_ShouldProcessAllElevators()
    {
        // Arrange
        _mockElevatorService.Setup(s => s.GetNextDestination(_testElevators[0])).Returns(5);
        _mockElevatorService.Setup(s => s.GetNextDestination(_testElevators[1])).Returns(10);
        _mockElevatorService.Setup(s => s.GetNextDestination(_testElevators[2])).Returns((int?)null);
        _mockElevatorService.Setup(s => s.GetNextDestination(_testElevators[3])).Returns((int?)null);

        // Act
        await _service.UpdateElevatorsAsync();

        // Assert
        _mockElevatorService.Verify(s => s.MoveToFloorAsync(_testElevators[0], 5), Times.Once);
        _mockElevatorService.Verify(s => s.MoveToFloorAsync(_testElevators[1], 10), Times.Once);
        _mockElevatorService.Verify(s => s.GetNextDestination(It.IsAny<Elevator>()), Times.Exactly(4));
    }

    [Test]
    public async Task ProcessMultipleRequestsConcurrentlyAsync_ShouldProcessAllRequests()
    {
        // Arrange
        var requests = new List<Request>
        {
            new(1, 5, 2),
            new(3, 8, 3),
            new(2, 10, 4)
        };

        _mockDispatchService.Setup(d => d.ProcessRequestAsync(It.IsAny<List<Elevator>>(), It.IsAny<Request>()))
            .ReturnsAsync(0);

        // Act
        var results = await _service.ProcessMultipleRequestsConcurrentlyAsync(requests);

        // Assert
        Assert.That(results.Count, Is.EqualTo(3));
        Assert.That(results.All(r => r == 0), Is.True);
        _mockDispatchService.Verify(d => d.ProcessRequestAsync(_testElevators, It.IsAny<Request>()), Times.Exactly(3));
    }

    [Test]
    public async Task ProcessQueuedRequestsAsync_ShouldProcessAvailableRequests()
    {
        // Arrange
        var testRequest = new Request(1, 5, 3);
        _mockQueueService.SetupSequence(q => q.HasPendingRequests)
            .Returns(true)
            .Returns(false);
        _mockQueueService.Setup(q => q.DequeueRequest()).Returns(testRequest);
        _mockDispatchService.Setup(d => d.ProcessRequestAsync(_testElevators, testRequest))
            .ReturnsAsync(0);

        // Act
        await _service.ProcessQueuedRequestsAsync();

        // Assert
        _mockQueueService.Verify(q => q.DequeueRequest(), Times.Once);
        _mockDispatchService.Verify(d => d.ProcessRequestAsync(_testElevators, testRequest), Times.Once);
    }

    [Test]
    public void Constructor_ShouldSubscribeObserversToAllElevators()
    {
        // Assert - Constructor should have been called in SetUp
        _mockObserverService.Verify(o => o.Subscribe(It.IsAny<int>(), It.IsAny<ElevatorLogger>()), Times.Exactly(4));
    }

    [Test]
    public void Constructor_ShouldThrowException_WhenDependencyIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SimulationService(
            null, _mockElevatorService.Object, _mockDispatchService.Object, 
            _mockObserverService.Object, _mockQueueService.Object));

        Assert.Throws<ArgumentNullException>(() => new SimulationService(
            _mockElevatorRepository.Object, null, _mockDispatchService.Object, 
            _mockObserverService.Object, _mockQueueService.Object));

        Assert.Throws<ArgumentNullException>(() => new SimulationService(
            _mockElevatorRepository.Object, _mockElevatorService.Object, null, 
            _mockObserverService.Object, _mockQueueService.Object));

        Assert.Throws<ArgumentNullException>(() => new SimulationService(
            _mockElevatorRepository.Object, _mockElevatorService.Object, _mockDispatchService.Object, 
            null, _mockQueueService.Object));

        Assert.Throws<ArgumentNullException>(() => new SimulationService(
            _mockElevatorRepository.Object, _mockElevatorService.Object, _mockDispatchService.Object, 
            _mockObserverService.Object, null));
    }
}