namespace ElevatorSimulator.Core.Application.Common.Models;

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