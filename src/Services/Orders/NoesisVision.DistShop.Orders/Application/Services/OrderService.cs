using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Orders.Domain.Aggregates;
using NoesisVision.DistShop.Orders.Domain.Repositories;
using NoesisVision.DistShop.Orders.Domain.ValueObjects;
using NoesisVision.DistShop.Orders.Domain.Exceptions;
using NoesisVision.DistShop.Orders.Application.DTOs;

namespace NoesisVision.DistShop.Orders.Application.Services;

/// <summary>
/// Service implementation for order operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderAggregate> CreateOrderAsync(Guid customerId, IEnumerable<OrderItemDto> items, string currency = "USD")
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", customerId);

            // Convert DTOs to domain value objects
            var orderItems = items.Select(dto => OrderItem.Create(
                dto.ProductId,
                dto.ProductName,
                dto.ProductSku,
                dto.Quantity,
                dto.UnitPrice,
                dto.Currency)).ToList();

            // Create the order aggregate
            var order = OrderAggregate.Create(customerId, orderItems, currency);

            // Save the order
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Publish domain events
            await PublishDomainEventsAsync(order);

            _logger.LogInformation("Successfully created order {OrderId} for customer {CustomerId}", order.Id, customerId);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<OrderAggregate?> GetOrderByIdAsync(Guid orderId)
    {
        return await _orderRepository.GetByIdAsync(orderId);
    }

    public async Task<IEnumerable<OrderAggregate>> GetOrdersByCustomerAsync(Guid customerId)
    {
        return await _orderRepository.GetByCustomerIdAsync(customerId);
    }

    public async Task<OrderAggregate> ConfirmOrderAsync(Guid orderId)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            throw new InvalidOrderOperationException($"Order {orderId} not found");

        order.Confirm();
        
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        await PublishDomainEventsAsync(order);
        
        _logger.LogInformation("Order {OrderId} confirmed", orderId);
        return order;
    }

    public async Task<OrderAggregate> StartProcessingOrderAsync(Guid orderId)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            throw new InvalidOrderOperationException($"Order {orderId} not found");

        order.StartProcessing();
        
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        await PublishDomainEventsAsync(order);
        
        _logger.LogInformation("Order {OrderId} processing started", orderId);
        return order;
    }

    public async Task<OrderAggregate> MarkOrderAsShippedAsync(Guid orderId)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            throw new InvalidOrderOperationException($"Order {orderId} not found");

        order.MarkAsShipped();
        
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        await PublishDomainEventsAsync(order);
        
        _logger.LogInformation("Order {OrderId} marked as shipped", orderId);
        return order;
    }

    public async Task<OrderAggregate> MarkOrderAsDeliveredAsync(Guid orderId)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            throw new InvalidOrderOperationException($"Order {orderId} not found");

        order.MarkAsDelivered();
        
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        await PublishDomainEventsAsync(order);
        
        _logger.LogInformation("Order {OrderId} marked as delivered", orderId);
        return order;
    }

    public async Task<OrderAggregate> CancelOrderAsync(Guid orderId, string reason)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            throw new InvalidOrderOperationException($"Order {orderId} not found");

        order.Cancel(reason);
        
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        await PublishDomainEventsAsync(order);
        
        _logger.LogInformation("Order {OrderId} cancelled with reason: {Reason}", orderId, reason);
        return order;
    }

    public async Task<IEnumerable<OrderAggregate>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _orderRepository.GetByStatusAsync(status);
    }

    private async Task PublishDomainEventsAsync(OrderAggregate order)
    {
        var domainEvents = order.DomainEvents.ToList();
        order.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            await _eventBus.PublishAsync(domainEvent);
        }
    }
}