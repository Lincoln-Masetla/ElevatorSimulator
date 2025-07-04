using ElevatorSimulator.Core.Application.Features.ElevatorManagement.Services;
using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Domain.Enums;
using ElevatorSimulator.Core.Application.Common.Interfaces;
using Moq;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class ElevatorObserverServiceTests
{
    private ElevatorObserverService _service;
    private Mock<IElevatorObserver> _mockObserver;
    private Elevator _elevator;

    [SetUp]
    public void SetUp()
    {
        _service = new ElevatorObserverService();
        _mockObserver = new Mock<IElevatorObserver>();
        _elevator = new Elevator { Id = 1, MaxCapacity = 8, CurrentFloor = 1 };
    }

    [Test]
    public void Subscribe_ShouldAddObserver()
    {
        // Act
        _service.Subscribe(_elevator.Id, _mockObserver.Object);
        _service.NotifyStateChanged(_elevator);

        // Assert
        _mockObserver.Verify(o => o.OnElevatorStateChanged(_elevator), Times.Once);
    }

    [Test]
    public void Subscribe_ShouldNotAddDuplicateObserver()
    {
        // Act
        _service.Subscribe(_elevator.Id, _mockObserver.Object);
        _service.Subscribe(_elevator.Id, _mockObserver.Object); // Duplicate
        _service.NotifyStateChanged(_elevator);

        // Assert
        _mockObserver.Verify(o => o.OnElevatorStateChanged(_elevator), Times.Once);
    }

    [Test]
    public void Unsubscribe_ShouldRemoveObserver()
    {
        // Arrange
        _service.Subscribe(_elevator.Id, _mockObserver.Object);

        // Act
        _service.Unsubscribe(_elevator.Id, _mockObserver.Object);
        _service.NotifyStateChanged(_elevator);

        // Assert
        _mockObserver.Verify(o => o.OnElevatorStateChanged(_elevator), Times.Never);
    }

    [Test]
    public void NotifyElevatorMoved_ShouldCallObserver()
    {
        // Arrange
        _service.Subscribe(_elevator.Id, _mockObserver.Object);
        var previousFloor = 1;

        // Act
        _service.NotifyElevatorMoved(_elevator, previousFloor);

        // Assert
        _mockObserver.Verify(o => o.OnElevatorMoved(_elevator, previousFloor), Times.Once);
    }

    [Test]
    public void NotifyPassengersChanged_ShouldCallObserver()
    {
        // Arrange
        _service.Subscribe(_elevator.Id, _mockObserver.Object);
        var previousCount = 0;

        // Act
        _service.NotifyPassengersChanged(_elevator, previousCount);

        // Assert
        _mockObserver.Verify(o => o.OnPassengersChanged(_elevator, previousCount), Times.Once);
    }

    [Test]
    public void NotifyDestinationAdded_ShouldCallObserver()
    {
        // Arrange
        _service.Subscribe(_elevator.Id, _mockObserver.Object);
        var floor = 5;

        // Act
        _service.NotifyDestinationAdded(_elevator, floor);

        // Assert
        _mockObserver.Verify(o => o.OnDestinationAdded(_elevator, floor), Times.Once);
    }

    [Test]
    public void NotifyDestinationReached_ShouldCallObserver()
    {
        // Arrange
        _service.Subscribe(_elevator.Id, _mockObserver.Object);
        var floor = 5;

        // Act
        _service.NotifyDestinationReached(_elevator, floor);

        // Assert
        _mockObserver.Verify(o => o.OnDestinationReached(_elevator, floor), Times.Once);
    }
}