using ElevatorSimulator.Core.Domain.Entities;
using ElevatorSimulator.Core.Domain.Enums;
using ElevatorSimulator.Core.Application.Common.Interfaces;

namespace ElevatorSimulator.Core.Application.Features.ElevatorManagement.Repositories;

/// <summary>
/// Repository for managing elevator instances - Single Responsibility: Elevator data management
/// </summary>
public class ElevatorRepository : IElevatorRepository
{
    private readonly List<Elevator> _elevators;

    public ElevatorRepository()
    {
        // Initialize building elevators - this could be moved to configuration
        _elevators = new List<Elevator>
        {
            new() { Id = 1, Type = ElevatorType.Standard, MaxCapacity = 8 },
            new() { Id = 2, Type = ElevatorType.Standard, MaxCapacity = 8 },
            new() { Id = 3, Type = ElevatorType.HighSpeed, MaxCapacity = 12 },
            new() { Id = 4, Type = ElevatorType.Freight, MaxCapacity = 20 }
        };
    }

    public List<Elevator> GetAll() => _elevators;

    public Elevator? GetById(int id) => _elevators.FirstOrDefault(e => e.Id == id);

    public List<Elevator> GetAvailable() 
        => _elevators.Where(e => e.State == ElevatorState.Idle && e.PassengerCount < e.MaxCapacity).ToList();
}