using NoesisVision.DistShop.SharedKernel.Domain;
using NoesisVision.DistShop.Orders.Domain.ValueObjects;
using NoesisVision.DistShop.Orders.Domain.Exceptions;
using NoesisVision.DistShop.Orders.Contracts.Events;

namespace NoesisVision.DistShop.Orders.Domain.Aggregates;

/// <summary>
/// Order aggregate root managing order lifecycle and state transitions
/// </summary>
public class OrderAggregate : AggregateRoot
{
    private readonly List<OrderItem> _items = new();
    private Guid _customerId;
    private OrderStatus _status;
    private decimal _totalAmount;
    private string _currency;
    private DateTime _createdAt;
    private DateTime _updatedAt;
    private string? _cancellationReason;

    // Valid state transitions
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions = new()
    {
        { OrderStatus.Pending, new HashSet<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
        { OrderStatus.Confirmed, new HashSet<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
        { OrderStatus.Processing, new HashSet<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled } },
        { OrderStatus.Shipped, new HashSet<OrderStatus> { OrderStatus.Delivered } },
        { OrderStatus.Delivered, new HashSet<OrderStatus>() }, // Terminal state
        { OrderStatus.Cancelled, new HashSet<OrderStatus>() }  // Terminal state
    };

    // EF Core constructor
    private OrderAggregate() : base()
    {
        _currency = null!;
    }

    private OrderAggregate(
        Guid customerId,
        IEnumerable<OrderItem> items,
        string currency) : base()
    {
        _customerId = customerId;
        _items.AddRange(items);
        _currency = currency ?? throw new ArgumentNullException(nameof(currency));
        _status = OrderStatus.Pending;
        _totalAmount = _items.Sum(item => item.TotalPrice);
        _createdAt = DateTime.UtcNow;
        _updatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCreatedEvent(
            Id,
            _customerId,
            _totalAmount,
            _currency,
            _createdAt));
    }

    public static OrderAggregate Create(
        Guid customerId,
        IEnumerable<OrderItem> items,
        string currency = "USD")
    {
        if (customerId == Guid.Empty)
            throw new InvalidOrderOperationException("Customer ID cannot be empty");

        var orderItems = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        
        if (!orderItems.Any())
            throw new InvalidOrderOperationException("Order must contain at least one item");

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidOrderOperationException("Currency cannot be null or empty");

        // Validate all items have the same currency
        if (orderItems.Any(item => item.Currency != currency))
            throw new InvalidOrderOperationException("All order items must have the same currency");

        return new OrderAggregate(customerId, orderItems, currency);
    }

    // Properties
    public Guid CustomerId => _customerId;
    public OrderStatus Status => _status;
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _totalAmount;
    public string Currency => _currency;
    public DateTime CreatedAt => _createdAt;
    public DateTime UpdatedAt => _updatedAt;
    public string? CancellationReason => _cancellationReason;

    // State transition methods
    public void Confirm()
    {
        ValidateTransition(OrderStatus.Confirmed);
        ChangeStatus(OrderStatus.Confirmed);
    }

    public void StartProcessing()
    {
        ValidateTransition(OrderStatus.Processing);
        ChangeStatus(OrderStatus.Processing);
    }

    public void MarkAsShipped()
    {
        ValidateTransition(OrderStatus.Shipped);
        ChangeStatus(OrderStatus.Shipped);
    }

    public void MarkAsDelivered()
    {
        ValidateTransition(OrderStatus.Delivered);
        ChangeStatus(OrderStatus.Delivered);
    }

    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOrderOperationException("Cancellation reason is required");

        ValidateTransition(OrderStatus.Cancelled);
        
        var previousStatus = _status;
        _status = OrderStatus.Cancelled;
        _cancellationReason = reason.Trim();
        _updatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledEvent(
            Id,
            _customerId,
            _cancellationReason,
            _updatedAt));

        AddDomainEvent(new OrderStatusChangedEvent(
            Id,
            previousStatus.ToString(),
            _status.ToString(),
            _updatedAt,
            _cancellationReason));
    }

    // Business logic methods
    public bool CanBeCancelled()
    {
        return ValidTransitions[_status].Contains(OrderStatus.Cancelled);
    }

    public bool IsInTerminalState()
    {
        return _status == OrderStatus.Delivered || _status == OrderStatus.Cancelled;
    }

    public bool IsActive()
    {
        return !IsInTerminalState();
    }

    // Private helper methods
    private void ValidateTransition(OrderStatus targetStatus)
    {
        if (!ValidTransitions[_status].Contains(targetStatus))
        {
            throw new InvalidOrderStateTransitionException(_status, targetStatus);
        }
    }

    private void ChangeStatus(OrderStatus newStatus)
    {
        var previousStatus = _status;
        _status = newStatus;
        _updatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedEvent(
            Id,
            previousStatus.ToString(),
            newStatus.ToString(),
            _updatedAt));
    }

    // Method to recalculate total (for potential future use)
    private void RecalculateTotal()
    {
        _totalAmount = _items.Sum(item => item.TotalPrice);
        _updatedAt = DateTime.UtcNow;
    }
}