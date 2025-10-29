using NoesisVision.DistShop.Catalog.Contracts.Events;
using NoesisVision.DistShop.Inventory.Application.Services;

namespace NoesisVision.DistShop.Inventory.Application.EventHandlers;

/// <summary>
/// Handles ProductCreatedEvent to automatically create inventory items for new products
/// </summary>
public class ProductCreatedEventHandler
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(
        IInventoryService inventoryService,
        ILogger<ProductCreatedEventHandler> logger)
    {
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the ProductCreatedEvent by creating a corresponding inventory item
    /// </summary>
    /// <param name="productCreatedEvent">The product created event</param>
    public async Task HandleAsync(ProductCreatedEvent productCreatedEvent)
    {
        try
        {
            _logger.LogInformation("Creating inventory item for new product {ProductId}", productCreatedEvent.ProductId);

            // Create inventory item with default values
            // In a real system, these might come from configuration or business rules
            const int defaultInitialQuantity = 0;
            const int defaultReorderLevel = 10;
            const int defaultMaxStockLevel = 1000;

            await _inventoryService.CreateInventoryItemAsync(
                productCreatedEvent.ProductId,
                defaultInitialQuantity,
                defaultReorderLevel,
                defaultMaxStockLevel);

            _logger.LogInformation("Successfully created inventory item for product {ProductId}", productCreatedEvent.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create inventory item for product {ProductId}", productCreatedEvent.ProductId);
            // In a production system, you might want to implement retry logic or dead letter queues
            throw;
        }
    }
}