using ElevatorSimulator.Core.Application.Common.Enums;

namespace ElevatorSimulator.Core.Application.Common.Models;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public LogCategory Category { get; set; }
    public string Message { get; set; } = "";
    public string FormattedMessage { get; set; } = "";
}