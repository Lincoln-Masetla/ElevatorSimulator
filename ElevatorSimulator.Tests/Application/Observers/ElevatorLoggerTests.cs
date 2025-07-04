using ElevatorSimulator.Core.Application.Common.Observers;
using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Domain.Enums;
using ElevatorSimulator.Core.Application.Common.Enums;
using ElevatorSimulator.Core.Application.Common.Models;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Observers;

[TestFixture]
public class ElevatorLoggerTests
{
    private ElevatorLogger _logger;
    private Elevator _elevator;

    [SetUp]
    public void SetUp()
    {
        _logger = new ElevatorLogger();
        _elevator = new Elevator { Id = 1, MaxCapacity = 8, CurrentFloor = 1 };
    }

    [Test]
    public void OnElevatorStateChanged_ShouldLogStateChange()
    {
        // Arrange
        _elevator.State = ElevatorState.Moving;
        _elevator.Direction = ElevatorDirection.Up;

        // Act
        _logger.OnElevatorStateChanged(_elevator);

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("Elevator 1"));
        Assert.That(logs[0], Contains.Substring("Moving"));
        Assert.That(logs[0], Contains.Substring("Up"));
    }

    [Test]
    public void OnElevatorMoved_ShouldLogMovement()
    {
        // Arrange
        _elevator.CurrentFloor = 5;

        // Act
        _logger.OnElevatorMoved(_elevator, 3);

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("Elevator 1"));
        Assert.That(logs[0], Contains.Substring("UP"));
        Assert.That(logs[0], Contains.Substring("floor 3 to floor 5"));
    }

    [Test]
    public void OnPassengersChanged_ShouldLogPassengerChange()
    {
        // Arrange
        _elevator.PassengerCount = 5;

        // Act
        _logger.OnPassengersChanged(_elevator, 2);

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("Elevator 1"));
        Assert.That(logs[0], Contains.Substring("Loaded 3 passengers"));
        Assert.That(logs[0], Contains.Substring("5/8"));
    }

    [Test]
    public void OnDestinationAdded_ShouldLogDestination()
    {
        // Arrange
        _elevator.Destinations.Add(5);

        // Act
        _logger.OnDestinationAdded(_elevator, 5);

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("Elevator 1"));
        Assert.That(logs[0], Contains.Substring("Destination added"));
        Assert.That(logs[0], Contains.Substring("Floor 5"));
    }

    [Test]
    public void LogRequestStart_ShouldLogRequestDetails()
    {
        // Arrange
        var request = new Request(3, 10, 5);

        // Act
        _logger.LogRequestStart(request, "Single");

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("REQUEST #1"));
        Assert.That(logs[0], Contains.Substring("[Single]"));
        Assert.That(logs[0], Contains.Substring("5 passengers"));
        Assert.That(logs[0], Contains.Substring("floor 3"));
        Assert.That(logs[0], Contains.Substring("UP"));
    }

    [Test]
    public void LogQueueOperation_ShouldLogQueueDetails()
    {
        // Arrange
        var request = new Request(5, 12, 8);

        // Act
        _logger.LogQueueOperation("ADDED", 1, request);

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("Queue ADDED"));
        Assert.That(logs[0], Contains.Substring("1 request(s)"));
        Assert.That(logs[0], Contains.Substring("8p 5 to 12"));
    }

    [Test]
    public void LogCapacityOverflow_ShouldLogOverflowDetails()
    {
        // Act
        _logger.LogCapacityOverflow(25, 20, 5);

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("Capacity overflow"));
        Assert.That(logs[0], Contains.Substring("Requested 25"));
        Assert.That(logs[0], Contains.Substring("Available 20"));
        Assert.That(logs[0], Contains.Substring("Queued 5"));
    }

    [Test]
    public void GetLogsByCategory_ShouldFilterCorrectly()
    {
        // Arrange
        var request = new Request(1, 5, 3);
        _logger.LogRequestStart(request, "Single");
        _logger.OnElevatorStateChanged(_elevator);
        _logger.LogQueueOperation("ADDED", 1, request);

        // Act
        var requestLogs = _logger.GetLogsByCategory(LogCategory.Request, 10);
        var stateLogs = _logger.GetLogsByCategory(LogCategory.ElevatorState, 10);

        // Assert
        Assert.That(requestLogs.Count, Is.EqualTo(1));
        Assert.That(stateLogs.Count, Is.EqualTo(1));
        Assert.That(requestLogs[0].Category, Is.EqualTo(LogCategory.Request));
        Assert.That(stateLogs[0].Category, Is.EqualTo(LogCategory.ElevatorState));
    }

    [Test]
    public void GetLogSummary_ShouldProvideCorrectStatistics()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);
        _logger.LogRequestStart(request1, "Single");
        _logger.LogRequestStart(request2, "Single");
        _logger.LogTripStart(1, "Standard", request1);
        _logger.LogCapacityOverflow(10, 8, 2); // Warning

        // Act
        var summary = _logger.GetLogSummary();

        // Assert
        Assert.That(summary.TotalRequests, Is.EqualTo(2));
        Assert.That(summary.TotalTrips, Is.EqualTo(1));
        Assert.That(summary.WarningCount, Is.EqualTo(1));
        Assert.That(summary.ErrorCount, Is.EqualTo(0));
        Assert.That(summary.TotalEntries, Is.GreaterThan(0));
    }

    [Test]
    public void ClearLogs_ShouldResetCounters()
    {
        // Arrange
        var request = new Request(1, 5, 3);
        _logger.LogRequestStart(request, "Single");
        _logger.LogTripStart(1, "Standard", request);

        // Act
        _logger.ClearLogs();

        // Assert
        var summary = _logger.GetLogSummary();
        Assert.That(summary.TotalEntries, Is.EqualTo(0));
        Assert.That(summary.TotalRequests, Is.EqualTo(0));
        Assert.That(summary.TotalTrips, Is.EqualTo(0));
        Assert.That(_logger.GetRecentLogs(10).Count, Is.EqualTo(0));
    }

    [Test]
    public void Logger_ShouldMaintainMaxLogEntries()
    {
        // Arrange & Act - Add more than 200 logs
        for (int i = 0; i < 250; i++)
        {
            var request = new Request(1, 5, 1);
            _logger.LogRequestStart(request, "Test");
        }

        // Assert
        var allLogs = _logger.GetAllLogs();
        Assert.That(allLogs.Count, Is.LessThanOrEqualTo(200)); // Should not exceed max
    }

    [Test]
    public void LogBatchOperation_ShouldLogBatchDetails()
    {
        // Act
        _logger.LogBatchOperation("STARTED", 5, 25);

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("Batch STARTED"));
        Assert.That(logs[0], Contains.Substring("5 trips"));
        Assert.That(logs[0], Contains.Substring("25 total passengers"));
    }

    [Test]
    public void LogSystemStats_ShouldLogStatistics()
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(2);

        // Act
        _logger.LogSystemStats(10, 50, duration);

        // Assert
        var logs = _logger.GetRecentLogs(1);
        Assert.That(logs.Count, Is.EqualTo(1));
        Assert.That(logs[0], Contains.Substring("Session Stats"));
        Assert.That(logs[0], Contains.Substring("10 trips"));
        Assert.That(logs[0], Contains.Substring("50 passengers"));
        Assert.That(logs[0], Contains.Substring("02:00"));
    }
}