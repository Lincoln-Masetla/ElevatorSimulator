using ElevatorSimulator.Domain.Entities;
using ElevatorSimulator.Domain.Interfaces;
using ElevatorSimulator.Domain.Enums;

namespace ElevatorSimulator.Application.Observers;

public class ElevatorLogger : IElevatorObserver
{
    private readonly List<LogEntry> _logEntries = new();
    private int _requestCounter = 0;
    private int _tripCounter = 0;

    public void OnElevatorStateChanged(Elevator elevator)
    {
        LogEvent(LogLevel.Info, LogCategory.ElevatorState, 
            $"Elevator {elevator.Id}: State changed to {elevator.State}, Direction: {elevator.Direction}");
    }

    public void OnElevatorMoved(Elevator elevator, int previousFloor)
    {
        var direction = elevator.CurrentFloor > previousFloor ? "UP" : "DOWN";
        LogEvent(LogLevel.Info, LogCategory.Movement, 
            $"Elevator {elevator.Id}: Moved {direction} from floor {previousFloor} to floor {elevator.CurrentFloor}");
    }

    public void OnPassengersChanged(Elevator elevator, int previousCount)
    {
        var change = elevator.PassengerCount - previousCount;
        var action = change > 0 ? "Loaded" : "Unloaded";
        LogEvent(LogLevel.Info, LogCategory.Passengers, 
            $"Elevator {elevator.Id}: {action} {Math.Abs(change)} passengers (Total: {elevator.PassengerCount}/{elevator.MaxCapacity})");
    }

    public void OnDestinationAdded(Elevator elevator, int floor)
    {
        LogEvent(LogLevel.Info, LogCategory.Dispatch, 
            $"Elevator {elevator.Id}: Destination added - Floor {floor} | Queue: [{string.Join(",", elevator.Destinations)}]");
    }

    public void OnDestinationReached(Elevator elevator, int floor)
    {
        LogEvent(LogLevel.Success, LogCategory.Completion, 
            $"Elevator {elevator.Id}: Reached destination - Floor {floor}");
    }

    // Additional logging methods for comprehensive tracking
    public void LogRequestStart(Request request, string mode = "Single")
    {
        _requestCounter++;
        var direction = request.Direction == ElevatorDirection.Up ? "UP" : "DOWN";
        LogEvent(LogLevel.System, LogCategory.Request, 
            $"REQUEST #{_requestCounter} [{mode}]: {request.PassengerCount} passengers from floor {request.FromFloor} to {request.ToFloor} ({direction})");
    }

    public void LogTripStart(int elevatorId, string elevatorType, Request request)
    {
        _tripCounter++;
        LogEvent(LogLevel.System, LogCategory.Trip, 
            $"TRIP #{_tripCounter}: Elevator {elevatorId} ({elevatorType}) assigned to serve {request.PassengerCount} passengers");
    }

    public void LogDispatchDecision(int elevatorId, string reason, double distance)
    {
        LogEvent(LogLevel.Debug, LogCategory.Dispatch, 
            $"Dispatch: Elevator {elevatorId} selected ({reason}) - Distance: {distance} floors");
    }

    public void LogQueueOperation(string operation, int queueSize, Request? request = null)
    {
        var details = request != null ? $" | Request: {request.PassengerCount}p {request.FromFloor} to {request.ToFloor}" : "";
        LogEvent(LogLevel.Warning, LogCategory.Queue, 
            $"Queue {operation}: {queueSize} request(s) in queue{details}");
    }

    public void LogCapacityOverflow(int requestedPassengers, int availableCapacity, int overflow)
    {
        LogEvent(LogLevel.Warning, LogCategory.Capacity, 
            $"Capacity overflow: Requested {requestedPassengers}, Available {availableCapacity}, Queued {overflow}");
    }

    public void LogBatchOperation(string operation, int tripCount, int totalPassengers)
    {
        LogEvent(LogLevel.System, LogCategory.Batch, 
            $"Batch {operation}: {tripCount} trips, {totalPassengers} total passengers");
    }

    public void LogSystemStats(int totalTrips, int totalPassengers, TimeSpan duration)
    {
        LogEvent(LogLevel.System, LogCategory.Statistics, 
            $"Session Stats: {totalTrips} trips completed, {totalPassengers} passengers transported in {duration:mm\\:ss}");
    }

    private void LogEvent(LogLevel level, LogCategory category, string message)
    {
        var timestamp = DateTime.Now;
        var logEntry = new LogEntry
        {
            Timestamp = timestamp,
            Level = level,
            Category = category,
            Message = message,
            FormattedMessage = $"[{timestamp:HH:mm:ss}] [{level}] [{category}] {message}"
        };
        
        _logEntries.Add(logEntry);
        
        // Keep only last 200 entries for comprehensive logging
        if (_logEntries.Count > 200)
        {
            _logEntries.RemoveAt(0);
        }
    }

    public List<string> GetRecentLogs(int count = 15)
    {
        return _logEntries.TakeLast(count).Select(e => e.FormattedMessage).ToList();
    }

    public List<LogEntry> GetLogsByCategory(LogCategory category, int count = 50)
    {
        return _logEntries.Where(e => e.Category == category).TakeLast(count).ToList();
    }

    public List<LogEntry> GetLogsByLevel(LogLevel level, int count = 50)
    {
        return _logEntries.Where(e => e.Level == level).TakeLast(count).ToList();
    }

    public List<LogEntry> GetAllLogs()
    {
        return _logEntries.ToList();
    }

    public void ClearLogs()
    {
        _logEntries.Clear();
        _requestCounter = 0;
        _tripCounter = 0;
    }

    public LogSummary GetLogSummary()
    {
        return new LogSummary
        {
            TotalEntries = _logEntries.Count,
            TotalRequests = _requestCounter,
            TotalTrips = _tripCounter,
            ErrorCount = _logEntries.Count(e => e.Level == LogLevel.Error),
            WarningCount = _logEntries.Count(e => e.Level == LogLevel.Warning),
            StartTime = _logEntries.FirstOrDefault()?.Timestamp ?? DateTime.Now,
            LastActivity = _logEntries.LastOrDefault()?.Timestamp ?? DateTime.Now
        };
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public LogCategory Category { get; set; }
    public string Message { get; set; } = "";
    public string FormattedMessage { get; set; } = "";
}

public class LogSummary
{
    public int TotalEntries { get; set; }
    public int TotalRequests { get; set; }
    public int TotalTrips { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastActivity { get; set; }
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Success,
    System
}

public enum LogCategory
{
    ElevatorState,
    Movement,
    Passengers,
    Dispatch,
    Completion,
    Request,
    Trip,
    Queue,
    Capacity,
    Batch,
    Statistics
}
