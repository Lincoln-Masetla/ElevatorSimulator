# DVT Elevator Challenge Simulator

A sophisticated real-time C# console application that simulates the movement of multiple elevators within a large building using clean architecture, SOLID principles, and intelligent dispatching algorithms.

## Table of Contents

1. [Project Overview](#project-overview)
2. [Features](#features)
3. [Getting Started](#getting-started)
   - [Prerequisites](#prerequisites)
   - [Installation](#installation)
   - [Running the Application](#running-the-application)
4. [Usage Guide](#usage-guide)
   - [Basic Operations](#basic-operations)
   - [Advanced Features](#advanced-features)
   - [Command Reference](#command-reference)
5. [Architecture & Design](#architecture--design)
6. [Layer-by-Layer Explanation](#layer-by-layer-explanation)
7. [Core Components](#core-components)
8. [Testing](#testing)
9. [Performance & Scalability](#performance--scalability)
10. [Contributing](#contributing)
11. [License](#license)
12. [Acknowledgments](#acknowledgments)

## Project Overview

The DVT Elevator Challenge Simulator is an enterprise-grade elevator management system designed to demonstrate advanced software engineering principles. The application simulates real-world elevator operations in a 20-floor building with 4 different types of elevators, each with unique characteristics and capacities.

### Business Context

Modern building management systems require sophisticated elevator control algorithms to efficiently transport passengers while minimizing wait times and energy consumption. This simulator provides a comprehensive platform for testing and analyzing elevator dispatch strategies, capacity management, and system performance under various load conditions.

### Key Objectives

- **Operational Efficiency**: Implement intelligent dispatching algorithms that minimize passenger wait times
- **Scalability**: Design a system that can easily accommodate additional elevators, floors, or building configurations
- **Real-time Processing**: Provide immediate feedback and live simulation of elevator movements
- **Data Analytics**: Generate comprehensive reports on system performance and usage patterns
- **User Experience**: Deliver an intuitive interface for both casual users and system administrators

## Features

### Core Functionality
- **Real-Time Elevator Simulation**: Live tracking of 4 elevators across 20 floors
- **Intelligent Dispatching**: Smart elevator assignment based on proximity and capacity
- **Multi-Elevator Coordination**: Automatic passenger distribution for large groups
- **Comprehensive Queueing**: FIFO request handling with overflow management
- **Detailed Analytics**: Trip summaries with elevator usage statistics

### Input Modes
- **Interactive Mode**: Guided step-by-step input with helpful hints
- **Quick Mode**: Single-line commands for rapid operation
- **Multi-Destination**: Batch processing of multiple trips
- **Batch Mode**: Advanced trip planning and execution

### Reporting & Analytics
- **Real-Time Status**: Live elevator positions and states
- **Performance Metrics**: Success rates, efficiency analysis
- **Usage Statistics**: Individual elevator utilization tracking
- **Capacity Analysis**: Load distribution and optimization insights

### Elevator Types
- **Standard Elevators** (2x): 8-passenger capacity, normal speed
- **High-Speed Elevator** (1x): 12-passenger capacity, faster movement
- **Freight Elevator** (1x): 20-passenger capacity, heavy-duty operation

## Getting Started

### Prerequisites

Before running the DVT Elevator Challenge Simulator, ensure you have the following installed:

- **.NET 8.0 SDK or later**
  - Download from [Microsoft .NET Downloads](https://dotnet.microsoft.com/download)
  - Verify installation: `dotnet --version`
- **Git** (for cloning the repository)
  - Download from [Git Downloads](https://git-scm.com/downloads)
- **Console/Terminal** with UTF-8 support
- **Minimum System Requirements**:
  - OS: Windows 10+, macOS 10.15+, or Linux (Ubuntu 18.04+)
  - RAM: 512 MB available
  - Storage: 100 MB free space

### Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Lincoln-Masetla/ElevatorSimulator.git
   cd ElevatorSimulator
   ```

2. **Verify Project Structure**
   ```
   ElevatorSimulator/
   ├── ElevatorSimulator/           # Main application
   ├── ElevatorSimulator.Tests/    # Unit tests
   ├── README.md                   # Documentation
   └── ElevatorSimulator.sln       # Solution file
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Solution**
   ```bash
   dotnet build
   ```

5. **Run Tests** (Optional but recommended)
   ```bash
   dotnet test
   ```

### Running the Application

#### Method 1: Using .NET CLI (Recommended)

1. **Navigate to Project Directory**
   ```bash
   cd ElevatorSimulator
   ```

2. **Run the Application**
   ```bash
   dotnet run
   ```

3. **Alternative: Run with Release Configuration**
   ```bash
   dotnet run --configuration Release
   ```

#### Method 2: Using Built Executable

1. **Build for Release**
   ```bash
   dotnet build --configuration Release
   ```

2. **Navigate to Output Directory**
   ```bash
   cd bin/Release/net8.0/
   ```

3. **Run the Executable**
   ```bash
   # Windows
   ElevatorSimulator.exe
   
   # macOS/Linux
   ./ElevatorSimulator
   ```

#### Method 3: Self-Contained Deployment

1. **Publish Self-Contained Application**
   ```bash
   # Windows
   dotnet publish -c Release -r win-x64 --self-contained
   
   # macOS
   dotnet publish -c Release -r osx-x64 --self-contained
   
   # Linux
   dotnet publish -c Release -r linux-x64 --self-contained
   ```

2. **Run from Publish Directory**
   ```bash
   cd bin/Release/net8.0/[runtime]/publish/
   ./ElevatorSimulator
   ```

## Usage Guide

### Basic Operations

#### 1. Welcome Screen
Upon starting the application, you'll see:
- Welcome message and instructions
- System initialization
- Main menu prompt

#### 2. Main Interface
The application displays:
- **Elevator Status Panel**: Real-time elevator positions and states
- **Command Interface**: Input options and instructions
- **Recent Activity Log**: Last 5 system operations

#### 3. First Steps
1. **Press Enter** to start the simulation
2. **Choose an input mode**:
   - Press ENTER for interactive guided mode
   - Type a command for quick mode
   - Type 'batch' for advanced planning

### Advanced Features

#### Interactive Mode (Recommended for Beginners)
1. Press ENTER at the main prompt
2. Follow step-by-step instructions:
   - Enter origin floor (1-20)
   - Enter destination floor (1-20)
   - Enter number of passengers
3. Confirm your request
4. Watch the real-time simulation

#### Quick Mode (For Experienced Users)
Use shorthand syntax for rapid commands:
- `8 5` - 5 passengers from floor 1 to floor 8
- `3 12 8` - 8 passengers from floor 3 to floor 12
- `15` - 1 passenger from floor 1 to floor 15

#### Multi-Destination Mode
Process multiple trips simultaneously:
- `3 8 12, 7 2 5` - Two trips: 12 passengers (3 to 8), 5 passengers (7 to 2)
- `1 15 8, 2 10 12, 5 18 6` - Three coordinated trips

#### Batch Mode
Plan complex scenarios:
1. Type `batch` at main prompt
2. Add multiple trips interactively
3. Review and modify your plan
4. Execute all trips with coordination

### Command Reference

| Command | Description | Example |
|---------|-------------|---------|
| `ENTER` | Interactive guided mode | Press Enter key |
| `[to] [passengers]` | Quick trip from floor 1 | `15 8` |
| `[from] [to] [passengers]` | Full trip specification | `3 12 5` |
| `[trip1], [trip2]` | Multiple destinations | `3 8 12, 7 2 5` |
| `batch` | Enter batch planning mode | `batch` |
| `multi` | Run demo scenario | `multi` |
| `logs` | View system logs | `logs` |
| `exit` | Quit application | `exit` |

#### Special Features
- **Smart Queueing**: Automatic request queuing when elevators are busy
- **Capacity Overflow**: Intelligent handling of passenger groups exceeding capacity
- **Real-Time Updates**: Live elevator tracking during operations
- **Error Recovery**: Graceful handling of invalid inputs with helpful messages

#### System Monitoring
Access comprehensive system information:
- **Log Viewer**: Filter by category (Requests, Trips, Capacity, etc.)
- **Performance Metrics**: Success rates, timing analysis
- **Usage Statistics**: Elevator utilization and efficiency
- **Queue Status**: Pending requests and processing status

## Architecture & Design

The application follows Clean Architecture principles, ensuring separation of concerns, testability, and maintainability. The design implements SOLID principles throughout, making the system extensible and robust.

### Clean Architecture Implementation

The system is structured in concentric layers, with dependencies flowing inward toward the domain core:

- **Domain Layer**: Contains the core business entities and rules
- **Application Layer**: Implements use cases and business logic
- **Infrastructure Layer**: Handles data persistence and external concerns
- **Presentation Layer**: Manages user interface and input/output

### SOLID Principles Application

- **Single Responsibility Principle**: Each class has a single, well-defined responsibility
- **Open/Closed Principle**: The system is open for extension but closed for modification
- **Liskov Substitution Principle**: All elevator types are interchangeable through common interfaces
- **Interface Segregation Principle**: Interfaces are focused and client-specific
- **Dependency Inversion Principle**: High-level modules depend on abstractions, not concretions

### Design Patterns

- **Observer Pattern**: Real-time notification system for elevator state changes
- **Strategy Pattern**: Different elevator types with varying behaviors
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Loose coupling between components
- **Command Pattern**: Request handling and processing

## Layer-by-Layer Explanation

### Domain Layer

The Domain Layer represents the heart of the application, containing the core business concepts and rules that define elevator operations.

#### Entities

**Elevator Entity**: Represents the physical elevator with properties including current floor, state, direction, passenger count, maximum capacity, and destination queue. The entity maintains its invariants and business rules, ensuring data consistency and valid state transitions.

**Request Entity**: Encapsulates passenger transportation requests with origin floor, destination floor, passenger count, and calculated direction. The entity automatically determines whether the request is for upward or downward movement.

**RequestQueue Entity**: Manages the first-in-first-out queue of pending requests when elevators are at capacity or unavailable. Provides thread-safe operations for enqueueing, dequeuing, and peeking at requests.

**MultiDestinationRequest Entity**: Handles complex scenarios where multiple trips are requested simultaneously, supporting batch processing and coordinated elevator dispatch.

#### Enumerations

**ElevatorState**: Defines the operational states including Idle, Moving, DoorsOpen, and OutOfService, ensuring proper state management throughout elevator operations.

**ElevatorDirection**: Specifies movement direction as Up, Down, or Idle, enabling direction-aware dispatching and optimization.

**ElevatorType**: Categorizes elevators as Standard, HighSpeed, or Freight, each with different capacity and speed characteristics.

#### Interfaces

**IElevatorService**: Defines elevator operational contracts including movement, passenger loading/unloading, and destination management.

**IDispatchService**: Specifies elevator selection and assignment algorithms for optimal resource utilization.

**ISimulationService**: Orchestrates overall system simulation including elevator coordination and request processing.

#### Exceptions

**CapacityExceededException**: Thrown when passenger loading exceeds elevator capacity limits.

**InvalidFloorException**: Raised for invalid floor numbers outside the building range.

### Application Layer

The Application Layer implements business use cases and coordinates between the domain and infrastructure layers.

#### Services

**AppService**: The primary orchestration service that manages user interactions, input processing, and workflow coordination. Handles various input modes including interactive prompts, quick commands, multi-destination requests, and batch processing. Provides comprehensive simulation summaries with detailed analytics.

**SimulationService**: Core simulation engine that manages elevator fleet operations, processes requests, and maintains system state. Implements the observer pattern for real-time notifications and coordinates between multiple elevators for optimal passenger distribution.

**ElevatorService**: Low-level elevator operations service handling movement mechanics, passenger boarding/alighting, destination management, and state transitions. Provides both synchronous and asynchronous APIs for different integration scenarios.

**DispatchService**: Implements sophisticated elevator assignment algorithms using proximity analysis, capacity optimization, and direction-aware routing. Handles multi-elevator coordination for large passenger groups and manages overflow scenarios.

**ElevatorObserverService**: Manages the observer pattern implementation, coordinating notifications between elevators and various observers including loggers and user interface components.

#### Observers

**ElevatorLogger**: Comprehensive logging system that tracks all elevator operations with categorized entries including requests, trips, movements, capacity changes, and system statistics. Provides filtering capabilities and performance analytics.

**ConsoleObserver**: Real-time console output observer that displays elevator state changes and system notifications during simulation.

#### Validators

**ElevatorRequestValidator**: Input validation service that ensures floor numbers are within valid ranges, passenger counts are positive, and requests meet business rules. Supports configurable validation parameters for different building configurations.

### Infrastructure Layer

The Infrastructure Layer handles external concerns and provides concrete implementations of domain interfaces.

#### Data Management

**In-Memory Storage**: High-performance data structures using collections for elevator state management, request queuing, and observer registrations. Optimized for real-time operations with minimal latency.

**Observer Registry**: Dictionary-based mapping system that efficiently manages relationships between elevators and their observers, enabling rapid notification distribution.

**Log Storage**: Circular buffer implementation that maintains recent activity logs while preventing excessive memory usage through automatic rotation.

### Presentation Layer

The Presentation Layer manages all user interactions and system output formatting.

#### Console Interface

**ConsoleUI**: Provides formatted display of elevator status, system information, and simulation progress. Implements clean, icon-free output with professional formatting and clear information hierarchy.

**Input Processing**: Sophisticated input parsing system that supports multiple command formats including guided interactive mode, quick shorthand commands, multi-destination syntax, and batch processing instructions.

**Output Formatting**: Comprehensive reporting system that generates detailed simulation summaries including trip analysis, elevator usage statistics, performance metrics, and capacity utilization reports.

## Core Components

### Elevator Management System

The elevator management system coordinates multiple elevators with different characteristics:

- **Standard Elevators**: 8-passenger capacity, normal speed, suitable for regular office traffic
- **High-Speed Elevators**: 12-passenger capacity, faster movement, ideal for express service
- **Freight Elevators**: 20-passenger capacity, slower speed, designed for heavy loads and large groups

### Intelligent Dispatching

The dispatching system implements advanced algorithms considering:

- **Proximity Analysis**: Selects the closest available elevator to minimize response time
- **Direction Preference**: Prioritizes elevators already moving in the request direction
- **Capacity Optimization**: Distributes passengers across multiple elevators for large groups
- **Load Balancing**: Prevents overloading of individual elevators

### Request Queue Management

The queueing system provides:

- **FIFO Processing**: First-in-first-out request handling for fairness
- **Overflow Handling**: Automatic queueing when system capacity is exceeded
- **Priority Management**: Intelligent processing of queued requests as elevators become available
- **Continuous Monitoring**: Real-time queue status and automatic processing

### Comprehensive Logging

The logging system captures:

- **Request Tracking**: Complete lifecycle of passenger requests
- **Movement Logging**: Detailed elevator movement and position changes
- **Capacity Monitoring**: Passenger loading and capacity utilization
- **Performance Metrics**: System efficiency and response time analytics
- **Error Handling**: Exception tracking and system diagnostics

## Features & Capabilities

### Input Modes

**Interactive Mode**: Step-by-step guided input with contextual hints and validation. Ideal for new users or when detailed assistance is needed.

**Quick Mode**: Shorthand command format supporting single-line requests with automatic parsing and validation. Enables rapid request entry for experienced users.

**Multi-Destination Mode**: Batch request processing supporting comma-separated trip lists with intelligent parsing and coordinated execution.

**Batch Mode**: Advanced trip planning interface allowing users to build complex scenarios with multiple requests before execution.

### Real-Time Simulation

**Live Status Display**: Real-time elevator position, state, and passenger information with automatic updates during simulation.

**Detailed Movement Tracking**: Step-by-step elevator movement simulation including floor-by-floor progression, door operations, and passenger boarding/alighting.

**Progress Indicators**: Clear visual feedback on trip progress including phase identification and estimated completion times.

### Analytics & Reporting

**Trip Analysis**: Detailed breakdown of each trip including distance, direction, elevator assignment, and passenger distribution.

**System Statistics**: Comprehensive metrics including total trips, passengers transported, average distances, and efficiency measures.

**Elevator Usage**: Individual elevator performance tracking with utilization percentages and activity summaries.

**Performance Monitoring**: Success rates, error tracking, and system health indicators.

### Advanced Features

**Smart Queueing**: Automatic request queueing with intelligent overflow handling and continuous processing as capacity becomes available.

**Multi-Elevator Coordination**: Sophisticated passenger distribution across multiple elevators for large groups, maximizing system efficiency.

**Capacity Management**: Real-time capacity monitoring with overflow detection and alternative routing suggestions.

**Error Recovery**: Robust error handling with graceful degradation and user-friendly error messages.


## Testing Strategy

### Unit Testing

**Component Testing**: Individual class and method testing with comprehensive coverage of business logic, edge cases, and error conditions.

**Service Testing**: Service layer testing with mocking of dependencies to ensure isolated functionality verification.

**Entity Testing**: Domain entity testing covering invariants, state transitions, and business rule enforcement.

### Integration Testing

**Service Integration**: Multi-service interaction testing to verify proper communication and data flow between application components.

**Observer Pattern Testing**: Notification system testing ensuring proper event propagation and handler execution.

**Dispatch Algorithm Testing**: Comprehensive testing of elevator selection and assignment logic under various scenarios.

### Scenario Testing

**Load Testing**: High-volume request processing to verify system performance under stress conditions.

**Edge Case Testing**: Boundary condition testing including maximum capacity, invalid inputs, and system limits.

**Error Condition Testing**: Exception handling verification and recovery mechanism validation.

### Test Coverage

The test suite includes over 95 test methods covering:
- **Elevator Operations**: Movement, loading, state management
- **Dispatch Logic**: Selection algorithms, multi-elevator coordination
- **Queue Management**: FIFO operations, overflow handling
- **Validation**: Input checking, business rule enforcement
- **Logging**: Event capture, filtering, reporting

## Performance & Scalability

### Performance Characteristics

**Response Time**: Sub-millisecond elevator selection and assignment for immediate user feedback.

**Memory Usage**: Efficient memory management with circular buffers and optimized data structures.

**Throughput**: Capable of processing hundreds of requests per second with minimal latency.

**Scalability**: Designed to support additional elevators and floors with linear performance scaling.

### Optimization Features

**Algorithm Efficiency**: O(n log n) complexity for elevator selection with optimized sorting and filtering.

**Memory Management**: Automatic cleanup and garbage collection optimization for long-running simulations.

**Resource Utilization**: Minimal CPU usage during idle periods with event-driven architecture.

**Data Structures**: Optimized collections and indexing for rapid data access and manipulation.

### Monitoring & Diagnostics

**Performance Metrics**: Built-in performance monitoring with timing analysis and resource utilization tracking.

**System Health**: Continuous monitoring of system state with automatic error detection and reporting.

**Capacity Planning**: Analytics for system sizing and configuration optimization based on usage patterns.

## Future Enhancements

### Planned Features

**Web Interface**: Browser-based user interface for remote access and improved user experience.

**REST API**: Web service interface for integration with external building management systems.

**Database Integration**: Persistent storage for historical data analysis and trend identification.

**Advanced Analytics**: Machine learning integration for predictive maintenance and optimization.

### Technology Upgrades

**Cloud Deployment**: Azure or AWS hosting for scalable, distributed operations.

**Microservices Architecture**: Service decomposition for improved scalability and maintainability.

**Real-time Dashboards**: Live monitoring interfaces with graphical displays and alerting systems.

**Mobile Applications**: iOS and Android apps for building management and monitoring.

### Operational Enhancements

**Maintenance Scheduling**: Preventive maintenance planning with service window optimization.

**Energy Optimization**: Power consumption monitoring and efficiency improvements.

**Security Features**: Access control, audit logging, and security compliance features.

**Multi-Building Support**: Campus-wide elevator management with centralized coordination.

---

*This simulator demonstrates modern software engineering practices while providing practical insights into elevator system design and optimization. The comprehensive architecture and thorough testing ensure reliability and maintainability for future enhancements and production deployment.*