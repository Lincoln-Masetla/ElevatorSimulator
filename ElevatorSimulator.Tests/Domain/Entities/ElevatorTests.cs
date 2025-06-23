using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Enums;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Domain.Entities;

[TestFixture]
public class ElevatorTests
{
    [Test]
    public void Constructor_ShouldSetDefaults()
    {
        var elevator = new Elevator();

        Assert.That(elevator.CurrentFloor, Is.EqualTo(1));
        Assert.That(elevator.Direction, Is.EqualTo(ElevatorDirection.Idle));
        Assert.That(elevator.State, Is.EqualTo(ElevatorState.Idle));
        Assert.That(elevator.PassengerCount, Is.EqualTo(0));
        Assert.That(elevator.Destinations, Is.Empty);
    }

    [Test]
    public void Properties_ShouldBeSettable()
    {
        var elevator = new Elevator
        {
            Id = 1,
            Type = ElevatorType.Standard,
            MaxCapacity = 8,
            CurrentFloor = 5,
            PassengerCount = 3
        };

        Assert.That(elevator.Id, Is.EqualTo(1));
        Assert.That(elevator.Type, Is.EqualTo(ElevatorType.Standard));
        Assert.That(elevator.MaxCapacity, Is.EqualTo(8));
        Assert.That(elevator.CurrentFloor, Is.EqualTo(5));
        Assert.That(elevator.PassengerCount, Is.EqualTo(3));
    }

    [Test]
    public void ElevatorType_ShouldHaveCorrectValues()
    {
        // Test different elevator types
        var standard = new Elevator { Type = ElevatorType.Standard, MaxCapacity = 8 };
        var highSpeed = new Elevator { Type = ElevatorType.HighSpeed, MaxCapacity = 12 };
        var freight = new Elevator { Type = ElevatorType.Freight, MaxCapacity = 20 };

        Assert.That(standard.Type, Is.EqualTo(ElevatorType.Standard));
        Assert.That(highSpeed.Type, Is.EqualTo(ElevatorType.HighSpeed));
        Assert.That(freight.Type, Is.EqualTo(ElevatorType.Freight));
    }

    [Test]
    public void ElevatorState_ShouldTransitionCorrectly()
    {
        var elevator = new Elevator();

        // Test state transitions
        elevator.State = ElevatorState.Moving;
        Assert.That(elevator.State, Is.EqualTo(ElevatorState.Moving));

        elevator.State = ElevatorState.DoorsOpen;
        Assert.That(elevator.State, Is.EqualTo(ElevatorState.DoorsOpen));

        elevator.State = ElevatorState.Idle;
        Assert.That(elevator.State, Is.EqualTo(ElevatorState.Idle));
    }

    [Test]
    public void ElevatorDirection_ShouldSetCorrectly()
    {
        var elevator = new Elevator();

        elevator.Direction = ElevatorDirection.Up;
        Assert.That(elevator.Direction, Is.EqualTo(ElevatorDirection.Up));

        elevator.Direction = ElevatorDirection.Down;
        Assert.That(elevator.Direction, Is.EqualTo(ElevatorDirection.Down));

        elevator.Direction = ElevatorDirection.Idle;
        Assert.That(elevator.Direction, Is.EqualTo(ElevatorDirection.Idle));
    }

    [Test]
    public void Destinations_ShouldBeManipulatable()
    {
        var elevator = new Elevator();

        // Add destinations
        elevator.Destinations.Add(5);
        elevator.Destinations.Add(10);
        elevator.Destinations.Add(3);

        Assert.That(elevator.Destinations.Count, Is.EqualTo(3));
        Assert.That(elevator.Destinations, Contains.Item(5));
        Assert.That(elevator.Destinations, Contains.Item(10));
        Assert.That(elevator.Destinations, Contains.Item(3));

        // Remove destination
        elevator.Destinations.Remove(10);
        Assert.That(elevator.Destinations.Count, Is.EqualTo(2));
        Assert.That(elevator.Destinations, Does.Not.Contain(10));
    }

    [Test]
    public void Elevator_ShouldHandleCapacityLimits()
    {
        var elevator = new Elevator { MaxCapacity = 8 };

        // Test within capacity
        elevator.PassengerCount = 5;
        Assert.That(elevator.PassengerCount <= elevator.MaxCapacity, Is.True);

        // Test at capacity
        elevator.PassengerCount = 8;
        Assert.That(elevator.PassengerCount, Is.EqualTo(elevator.MaxCapacity));
    }
}