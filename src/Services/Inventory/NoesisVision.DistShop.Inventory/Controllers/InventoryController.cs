using Microsoft.AspNetCore.Mvc;
using NoesisVision.DistShop.Inventory.Application.Services;
using NoesisVision.DistShop.Inventory.Application.DTOs;
using NoesisVision.DistShop.Inventory.Domain.Exceptions;

namespace NoesisVision.DistShop.Inventory.Controllers;

/// <summary>
/// API controller for inventory operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryService inventoryService,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets inventory information for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Inventory item information</returns>
    [HttpGet("products/{productId:guid}")]
    public async Task<ActionResult<InventoryItemDto>> GetInventoryByProductId(Guid productId)
    {
        try
        {
            var inventoryItem = await _inventoryService.GetInventoryByProductIdAsync(productId);
            
            if (inventoryItem == null)
            {
                return NotFound($"Inventory not found for product {productId}");
            }

            var dto = MapToDto(inventoryItem);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory for product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new inventory item for a product
    /// </summary>
    /// <param name="request">Create inventory request</param>
    /// <returns>Created inventory item</returns>
    [HttpPost]
    public async Task<ActionResult<InventoryItemDto>> CreateInventoryItem([FromBody] CreateInventoryItemRequest request)
    {
        try
        {
            var inventoryItem = await _inventoryService.CreateInventoryItemAsync(
                request.ProductId,
                request.InitialQuantity,
                request.ReorderLevel,
                request.MaxStockLevel);

            var dto = MapToDto(inventoryItem);
            return CreatedAtAction(nameof(GetInventoryByProductId), new { productId = request.ProductId }, dto);
        }
        catch (InventoryDomainException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory item for product {ProductId}", request.ProductId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Reserves stock for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="request">Reserve stock request</param>
    /// <returns>Stock reservation information</returns>
    [HttpPost("products/{productId:guid}/reservations")]
    public async Task<ActionResult<StockReservationDto>> ReserveStock(Guid productId, [FromBody] ReserveStockRequest request)
    {
        try
        {
            var reservation = await _inventoryService.ReserveStockAsync(
                productId,
                request.Quantity,
                TimeSpan.FromMinutes(request.DurationMinutes),
                request.Reference);

            var dto = new StockReservationDto
            {
                ReservationId = reservation.ReservationId,
                Quantity = reservation.Quantity,
                CreatedAt = reservation.CreatedAt,
                ExpiresAt = reservation.ExpiresAt,
                Reference = reservation.Reference,
                IsExpired = reservation.IsExpired
            };

            return Ok(dto);
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new { error = "Insufficient stock", details = ex.Message });
        }
        catch (InventoryDomainException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Releases a stock reservation
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="reservationId">Reservation ID</param>
    /// <param name="request">Release reservation request</param>
    /// <returns>Success result</returns>
    [HttpDelete("products/{productId:guid}/reservations/{reservationId:guid}")]
    public async Task<ActionResult> ReleaseReservation(Guid productId, Guid reservationId, [FromBody] ReleaseReservationRequest? request = null)
    {
        try
        {
            await _inventoryService.ReleaseReservationAsync(productId, reservationId, request?.Reason ?? "Manual release");
            return NoContent();
        }
        catch (InvalidReservationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InventoryDomainException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing reservation {ReservationId} for product {ProductId}", reservationId, productId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Confirms a stock reservation (permanently removes stock)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="reservationId">Reservation ID</param>
    /// <returns>Success result</returns>
    [HttpPost("products/{productId:guid}/reservations/{reservationId:guid}/confirm")]
    public async Task<ActionResult> ConfirmReservation(Guid productId, Guid reservationId)
    {
        try
        {
            await _inventoryService.ConfirmReservationAsync(productId, reservationId);
            return NoContent();
        }
        catch (InvalidReservationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InventoryDomainException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming reservation {ReservationId} for product {ProductId}", reservationId, productId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Adjusts stock levels for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="request">Stock adjustment request</param>
    /// <returns>Success result</returns>
    [HttpPost("products/{productId:guid}/adjustments")]
    public async Task<ActionResult> AdjustStock(Guid productId, [FromBody] AdjustStockRequest request)
    {
        try
        {
            await _inventoryService.AdjustStockAsync(productId, request.QuantityChange, request.Reason);
            return NoContent();
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new { error = "Insufficient stock", details = ex.Message });
        }
        catch (InventoryDomainException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock for product {ProductId}", productId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets all inventory items with low stock
    /// </summary>
    /// <returns>List of low stock items</returns>
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetLowStockItems()
    {
        try
        {
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
            var dtos = lowStockItems.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock items");
            return StatusCode(500, "Internal server error");
        }
    }

    private static InventoryItemDto MapToDto(Domain.Aggregates.InventoryItem inventoryItem)
    {
        return new InventoryItemDto
        {
            Id = inventoryItem.Id,
            ProductId = inventoryItem.ProductId,
            AvailableQuantity = inventoryItem.AvailableQuantity,
            ReservedQuantity = inventoryItem.ReservedQuantity,
            TotalQuantity = inventoryItem.TotalQuantity,
            ReorderLevel = inventoryItem.ReorderLevel,
            MaxStockLevel = inventoryItem.MaxStockLevel,
            LastUpdated = inventoryItem.LastUpdated,
            ActiveReservations = inventoryItem.Reservations.Select(r => new StockReservationDto
            {
                ReservationId = r.ReservationId,
                Quantity = r.Quantity,
                CreatedAt = r.CreatedAt,
                ExpiresAt = r.ExpiresAt,
                Reference = r.Reference,
                IsExpired = r.IsExpired
            }).ToList()
        };
    }
}

// Request DTOs
public class CreateInventoryItemRequest
{
    public Guid ProductId { get; set; }
    public int InitialQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public int MaxStockLevel { get; set; }
}

public class ReserveStockRequest
{
    public int Quantity { get; set; }
    public int DurationMinutes { get; set; } = 30; // Default 30 minutes
    public string? Reference { get; set; }
}

public class ReleaseReservationRequest
{
    public string Reason { get; set; } = "Manual release";
}

public class AdjustStockRequest
{
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = "Manual adjustment";
}