using Microsoft.AspNetCore.Mvc;
using NoesisVision.DistShop.Orders.Application.Services;
using NoesisVision.DistShop.Orders.Application.DTOs;
using NoesisVision.DistShop.Orders.Domain.ValueObjects;
using NoesisVision.DistShop.Orders.Domain.Exceptions;

namespace NoesisVision.DistShop.Orders.Controllers;

/// <summary>
/// REST API controller for order operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="request">The order creation request</param>
    /// <returns>The created order</returns>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(
                request.CustomerId,
                request.Items,
                request.Currency);

            var orderDto = MapToDto(order);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
        }
        catch (InvalidOrderOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", request.CustomerId);
            return StatusCode(500, new { error = "An error occurred while creating the order" });
        }
    }

    /// <summary>
    /// Gets an order by its identifier
    /// </summary>
    /// <param name="id">The order identifier</param>
    /// <returns>The order if found</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return Ok(MapToDto(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get order {OrderId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the order" });
        }
    }

    /// <summary>
    /// Gets all orders for a customer
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <returns>Collection of orders for the customer</returns>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomer(Guid customerId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            var orderDtos = orders.Select(MapToDto);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get orders for customer {CustomerId}", customerId);
            return StatusCode(500, new { error = "An error occurred while retrieving orders" });
        }
    }

    /// <summary>
    /// Gets orders by status
    /// </summary>
    /// <param name="status">The order status</param>
    /// <returns>Collection of orders with the specified status</returns>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByStatus(OrderStatus status)
    {
        try
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            var orderDtos = orders.Select(MapToDto);
            return Ok(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get orders with status {Status}", status);
            return StatusCode(500, new { error = "An error occurred while retrieving orders" });
        }
    }

    /// <summary>
    /// Confirms an order
    /// </summary>
    /// <param name="id">The order identifier</param>
    /// <returns>The updated order</returns>
    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<OrderDto>> ConfirmOrder(Guid id)
    {
        try
        {
            var order = await _orderService.ConfirmOrderAsync(id);
            return Ok(MapToDto(order));
        }
        catch (InvalidOrderOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOrderStateTransitionException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm order {OrderId}", id);
            return StatusCode(500, new { error = "An error occurred while confirming the order" });
        }
    }

    /// <summary>
    /// Starts processing an order
    /// </summary>
    /// <param name="id">The order identifier</param>
    /// <returns>The updated order</returns>
    [HttpPost("{id}/process")]
    public async Task<ActionResult<OrderDto>> StartProcessingOrder(Guid id)
    {
        try
        {
            var order = await _orderService.StartProcessingOrderAsync(id);
            return Ok(MapToDto(order));
        }
        catch (InvalidOrderOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOrderStateTransitionException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start processing order {OrderId}", id);
            return StatusCode(500, new { error = "An error occurred while processing the order" });
        }
    }

    /// <summary>
    /// Marks an order as shipped
    /// </summary>
    /// <param name="id">The order identifier</param>
    /// <returns>The updated order</returns>
    [HttpPost("{id}/ship")]
    public async Task<ActionResult<OrderDto>> MarkOrderAsShipped(Guid id)
    {
        try
        {
            var order = await _orderService.MarkOrderAsShippedAsync(id);
            return Ok(MapToDto(order));
        }
        catch (InvalidOrderOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOrderStateTransitionException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark order {OrderId} as shipped", id);
            return StatusCode(500, new { error = "An error occurred while updating the order" });
        }
    }

    /// <summary>
    /// Marks an order as delivered
    /// </summary>
    /// <param name="id">The order identifier</param>
    /// <returns>The updated order</returns>
    [HttpPost("{id}/deliver")]
    public async Task<ActionResult<OrderDto>> MarkOrderAsDelivered(Guid id)
    {
        try
        {
            var order = await _orderService.MarkOrderAsDeliveredAsync(id);
            return Ok(MapToDto(order));
        }
        catch (InvalidOrderOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOrderStateTransitionException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark order {OrderId} as delivered", id);
            return StatusCode(500, new { error = "An error occurred while updating the order" });
        }
    }

    /// <summary>
    /// Cancels an order
    /// </summary>
    /// <param name="id">The order identifier</param>
    /// <param name="request">The cancellation request</param>
    /// <returns>The updated order</returns>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderDto>> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        try
        {
            var order = await _orderService.CancelOrderAsync(id, request.Reason);
            return Ok(MapToDto(order));
        }
        catch (InvalidOrderOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOrderStateTransitionException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel order {OrderId}", id);
            return StatusCode(500, new { error = "An error occurred while cancelling the order" });
        }
    }

    private static OrderDto MapToDto(Domain.Aggregates.OrderAggregate order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            Items = order.Items.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductSku = item.ProductSku,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Currency = item.Currency
            }).ToList(),
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            CancellationReason = order.CancellationReason
        };
    }
}

/// <summary>
/// Request model for creating an order
/// </summary>
public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Request model for cancelling an order
/// </summary>
public class CancelOrderRequest
{
    public string Reason { get; set; } = null!;
}