using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.Orders.Contracts.Events;

namespace NoesisVision.DistShop.Orders.Application.EventHandlers;

/// <summary>
/// Handles OrderStatusChangedEvent to coordinate with other services
/// </summary>
public class OrderStatusChangedEventHandler
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(
        IEventBus eventBus,
        ILogger<OrderStatusChangedEventHandler> logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the OrderStatusChangedEvent to coordinate with other services
    /// </summary>
    /// <param name="orderStatusChangedEvent">The order status changed event</param>
    public Task HandleAsync(OrderStatusChangedEvent orderStatusChangedEvent)
    {
        try
        {
            _logger.LogInformation("Processing order status change for order {OrderId}: {PreviousStatus} -> {NewStatus}", 
                orderStatusChangedEvent.OrderId, orderStatusChangedEvent.PreviousStatus, orderStatusChangedEvent.NewStatus);

            // Coordinate with other services based on status changes
            switch (orderStatusChangedEvent.NewStatus)
            {
                case "Confirmed":
                    _logger.LogInformation("Order {OrderId} confirmed - inventory service should reserve stock", 
                        orderStatusChangedEvent.OrderId);
                    // In a real implementation, you might publish a StockReservationRequestedEvent
                    break;

                case "Processing":
                    _logger.LogInformation("Order {OrderId} processing - payment service should process payment", 
                        orderStatusChangedEvent.OrderId);
                    // In a real implementation, you might publish a PaymentRequestedEvent
                    break;

                case "Shipped":
                    _logger.LogInformation("Order {OrderId} shipped - shipment service should create tracking", 
                        orderStatusChangedEvent.OrderId);
                    // In a real implementation, you might publish a ShipmentCreatedEvent
                    break;

                case "Delivered":
                    _logger.LogInformation("Order {OrderId} delivered - order lifecycle complete", 
                        orderStatusChangedEvent.OrderId);
                    break;

                case "Cancelled":
                    _logger.LogInformation("Order {OrderId} cancelled - inventory service should release reserved stock", 
                        orderStatusChangedEvent.OrderId);
                    // In a real implementation, you might publish a StockReservationReleasedEvent
                    break;
            }

            _logger.LogInformation("Successfully processed order status change for order {OrderId}", 
                orderStatusChangedEvent.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order status change for order {OrderId}", 
                orderStatusChangedEvent.OrderId);
            throw;
        }

        return Task.CompletedTask;
    }
}