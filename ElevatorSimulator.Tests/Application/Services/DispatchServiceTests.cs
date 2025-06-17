using ElevatorSimulator.Application.Services;
using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using ElevatorSimulator.Domain.Interfaces;
using Moq;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class DispatchServiceTests
{
    private DispatchService _service;
    private Mock<IElevatorService> _mockElevatorService;
    private List<Elevator> _elevators;

    [SetUp]
    public void SetUp()
    {
        _mockElevatorService = new Mock<IElevatorService>();
        _service = new DispatchService(_mockElevatorService.Object);
        _elevators = new List<Elevator>
        {
            new() { Id = 1, MaxCapacity = 8, CurrentFloor = 1, State = ElevatorState.Idle },
            new() { Id = 2, MaxCapacity = 8, CurrentFloor = 5, State = ElevatorState.Idle },
            new() { Id = 3, MaxCapacity = 12, CurrentFloor = 10, State = ElevatorState.Idle, Type = ElevatorType.HighSpeed },
            new() { Id = 4, MaxCapacity = 20, CurrentFloor = 1, State = ElevatorState.Idle, Type = ElevatorType.Freight }
        };
    }

    [Test]
    public void FindBestElevator_ShouldReturnClosest()
    {
        // Arrange
        var request = new Request(1, 4, 1);
        _mockElevatorService.Setup(s => s.CanAcceptPassengers(It.IsAny<Elevator>(), It.IsAny<int>())).Returns(true);
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[0], 1)).Returns(0);
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[1], 1)).Returns(4);

        // Act
        var result = _service.FindBestElevator(_elevators, request);

        // Assert
        Assert.That(result?.Id, Is.EqualTo(1)); // Elevator 1 is closest to floor 1
    }

    [Test]
    public void FindBestElevator_ShouldReturnNull_WhenNoneAvailable()
    {
        // Arrange - all elevators at capacity
        _elevators[0].PassengerCount = 8;
        _elevators[1].PassengerCount = 8;
        _elevators[2].PassengerCount = 12;
        _elevators[3].PassengerCount = 20;
        var request = new Request(1, 3, 1);

        _mockElevatorService.Setup(s => s.CanAcceptPassengers(It.IsAny<Elevator>(), It.IsAny<int>())).Returns(false);

        // Act
        var result = _service.FindBestElevator(_elevators, request);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ProcessRequestAsync_ShouldQueueExcessPassengers()
    {
        // Arrange
        var request = new Request(1, 15, 60); // 60 passengers - exceeds total capacity (48)
        _mockElevatorService.Setup(s => s.CanAcceptPassengers(It.IsAny<Elevator>(), It.IsAny<int>())).Returns(true);
        _mockElevatorService.Setup(s => s.CalculateDistance(It.IsAny<Elevator>(), It.IsAny<int>())).Returns(1);

        // Act
        var remainingPassengers = await _service.ProcessRequestAsync(_elevators, request);

        // Assert - should queue 12 passengers (60 - 48)
        Assert.That(remainingPassengers, Is.EqualTo(12));
    }

    [Test]
    public void FindClosestElevator_ShouldReturnClosestElevator()
    {
        // Arrange
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[0], 7)).Returns(6);
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[1], 7)).Returns(2);
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[2], 7)).Returns(3);
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[3], 7)).Returns(6);

        // Act
        var result = _service.FindClosestElevator(_elevators, 7);

        // Assert
        Assert.That(result?.Id, Is.EqualTo(2)); // Elevator 2 is closest to floor 7
    }

    [Test]
    public void FindClosestElevator_ShouldIgnoreOutOfServiceElevators()
    {
        // Arrange
        _elevators[1].State = ElevatorState.OutOfService; // Closest elevator is out of service
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[0], 7)).Returns(6);
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[2], 7)).Returns(3);
        _mockElevatorService.Setup(s => s.CalculateDistance(_elevators[3], 7)).Returns(6);

        // Act
        var result = _service.FindClosestElevator(_elevators, 7);

        // Assert
        Assert.That(result?.Id, Is.EqualTo(3)); // Should skip out-of-service elevator
    }
}