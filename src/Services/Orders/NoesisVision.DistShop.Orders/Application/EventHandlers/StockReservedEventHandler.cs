using NoesisVision.DistShop.Inventory.Contracts.Events;
using NoesisVision.DistShop.Orders.Application.Services;

namespace NoesisVision.DistShop.Orders.Application.EventHandlers;

/// <summary>
/// Handles StockReservedEvent to update order processing status
/// </summary>
public class StockReservedEventHandler
{
    private readonly IOrderService _orderService;
    private readonly ILogger<StockReservedEventHandler> _logger;

    public StockReservedEventHandler(
        IOrderService orderService,
        ILogger<StockReservedEventHandler> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the StockReservedEvent by potentially updating order status
    /// </summary>
    /// <param name="stockReservedEvent">The stock reserved event</param>
    public Task HandleAsync(StockReservedEvent stockReservedEvent)
    {
        try
        {
            _logger.LogInformation("Processing stock reservation for product {ProductId}, reservation {ReservationId}", 
                stockReservedEvent.ProductId, stockReservedEvent.ReservationId);

            // In a real implementation, you would:
            // 1. Find orders that are waiting for this product's stock reservation
            // 2. Check if all items in those orders now have stock reserved
            // 3. Move those orders to the next status (e.g., from Confirmed to Processing)
            
            // For this demo, we'll just log the event
            _logger.LogInformation("Stock reserved for product {ProductId}: {Quantity} units, reservation expires at {ExpiresAt}", 
                stockReservedEvent.ProductId, stockReservedEvent.Quantity, stockReservedEvent.ExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process stock reservation for product {ProductId}", stockReservedEvent.ProductId);
            throw;
        }

        return Task.CompletedTask;
    }
}