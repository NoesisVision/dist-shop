# NoesisVision DistShop E-commerce Platform

**THIS PROJECT IS ENTIRELY AI GENERATED AND IS HERE ONLY FOR THE PURPOSE OF DEMONSTRATING NOESIS ARCHITECTURE VISUALIZATION - PLEASE DO NOT USE ITS CONTENT IN PRODUCTION SYSTEMS!**

A distributed e-commerce platform built using .NET 9 microservices architecture following Domain-Driven Design (DDD) principles.

## Solution Structure

```
NoesisVision.DistShop/
├── src/
│   ├── Shared/
│   │   └── NoesisVision.DistShop.SharedKernel/     # Common domain patterns and abstractions
│   └── Services/
│       ├── Catalog/                                # Product catalog service
│       ├── Inventory/                              # Inventory management service
│       ├── Pricing/                                # Pricing engine service
│       ├── Orders/                                 # Order management service
│       ├── OnlineShopping/                         # Shopping cart and checkout service
│       ├── Payments/                               # Payment processing service
│       ├── Shipment/                               # Shipment tracking service
│       ├── Wholesale/                              # Wholesale operations service
│       └── SearchEngine/                           # Product search service
├── Directory.Build.props                           # Solution-wide build configuration
├── global.json                                     # .NET SDK version specification
└── NoesisVision.DistShop.sln                      # Solution file
```

## Shared Kernel

The `NoesisVision.DistShop.SharedKernel` project contains:

### Domain Building Blocks
- `AggregateRoot` - Base class for domain aggregates with domain event support
- `Entity` - Base class for domain entities with identity
- `ValueObject` - Base class for value objects with equality semantics
- `DomainException` - Base exception class for domain-specific exceptions

### CQRS Abstractions
- `IEvent` - Base interface for all events
- `ICommand` - Base interface for all commands
- `IQuery<TResult>` - Base interface for all queries

### Infrastructure Abstractions
- `IRepository<T>` - Generic repository interface for aggregate roots
- `IUnitOfWork` - Unit of work pattern interface
- `IEventBus` - Event bus abstraction for inter-service communication

### Event Bus Implementation
- `InMemoryEventBus` - Simple in-memory event bus implementation for demonstration

## Naming Conventions

- **Events**: Use past tense with "Event" suffix (e.g., `ProductAddedToCartEvent`)
- **Commands**: Use imperative with "Command" suffix (e.g., `AddProductToCartCommand`)
- **Queries**: Use descriptive with "Query" suffix (e.g., `GetCartQuery`)

## Getting Started

1. Ensure you have .NET 9 SDK installed
2. Clone the repository
3. Run `dotnet build` to build the solution
4. Individual services will be implemented as separate projects within their respective folders

## Architecture Principles

- **Domain-Driven Design**: Each service represents a bounded context
- **Event-Driven Architecture**: Services communicate through domain events
- **CQRS**: Clear separation between commands and queries
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Loose coupling through DI container
