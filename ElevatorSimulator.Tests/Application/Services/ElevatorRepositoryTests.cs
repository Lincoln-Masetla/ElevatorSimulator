using ElevatorSimulator.Core.Application.Features.ElevatorManagement.Repositories;
using ElevatorSimulator.Core.Domain.Enums;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class ElevatorRepositoryTests
{
    private ElevatorRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _repository = new ElevatorRepository();
    }

    [Test]
    public void GetAll_ShouldReturn4Elevators()
    {
        // Act
        var elevators = _repository.GetAll();

        // Assert
        Assert.That(elevators.Count, Is.EqualTo(4));
        Assert.That(elevators.Count(e => e.Type == ElevatorType.Standard), Is.EqualTo(2));
        Assert.That(elevators.Count(e => e.Type == ElevatorType.HighSpeed), Is.EqualTo(1));
        Assert.That(elevators.Count(e => e.Type == ElevatorType.Freight), Is.EqualTo(1));
    }

    [Test]
    public void GetAll_ShouldHaveCorrectCapacities()
    {
        // Act
        var elevators = _repository.GetAll();

        // Assert
        Assert.That(elevators.Where(e => e.Type == ElevatorType.Standard).All(e => e.MaxCapacity == 8), Is.True);
        Assert.That(elevators.Where(e => e.Type == ElevatorType.HighSpeed).All(e => e.MaxCapacity == 12), Is.True);
        Assert.That(elevators.Where(e => e.Type == ElevatorType.Freight).All(e => e.MaxCapacity == 20), Is.True);
    }

    [Test]
    public void GetById_ShouldReturnCorrectElevator()
    {
        // Act
        var elevator = _repository.GetById(1);

        // Assert
        Assert.That(elevator, Is.Not.Null);
        Assert.That(elevator.Id, Is.EqualTo(1));
        Assert.That(elevator.Type, Is.EqualTo(ElevatorType.Standard));
    }

    [Test]
    public void GetById_ShouldReturnNull_WhenElevatorNotFound()
    {
        // Act
        var elevator = _repository.GetById(999);

        // Assert
        Assert.That(elevator, Is.Null);
    }

    [Test]
    public void GetAvailable_ShouldReturnIdleElevatorsWithCapacity()
    {
        // Arrange
        var elevators = _repository.GetAll();
        elevators[0].State = ElevatorState.Moving; // Make first elevator unavailable
        elevators[1].PassengerCount = elevators[1].MaxCapacity; // Make second elevator at capacity

        // Act
        var available = _repository.GetAvailable();

        // Assert
        Assert.That(available.Count, Is.EqualTo(2)); // Only 2 should be available
        Assert.That(available.All(e => e.State == ElevatorState.Idle), Is.True);
        Assert.That(available.All(e => e.PassengerCount < e.MaxCapacity), Is.True);
    }

    [Test]
    public void GetAvailable_ShouldReturnEmptyList_WhenNoElevatorsAvailable()
    {
        // Arrange
        var elevators = _repository.GetAll();
        foreach (var elevator in elevators)
        {
            elevator.State = ElevatorState.Moving; // Make all elevators unavailable
        }

        // Act
        var available = _repository.GetAvailable();

        // Assert
        Assert.That(available.Count, Is.EqualTo(0));
    }
}