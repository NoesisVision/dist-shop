using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.OnlineShopping.Application.DTOs;
using NoesisVision.DistShop.OnlineShopping.Domain.Repositories;
using NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;

namespace NoesisVision.DistShop.OnlineShopping.Application.Services;

/// <summary>
/// Service for orchestrating checkout process (Checkout Saga)
/// </summary>
public class CheckoutService : ICheckoutService
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IPricingIntegrationService _pricingService;
    private readonly IInventoryIntegrationService _inventoryService;
    private readonly IOrderIntegrationService _orderService;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IPricingIntegrationService pricingService,
        IInventoryIntegrationService inventoryService,
        IOrderIntegrationService orderService,
        ILogger<CheckoutService> logger)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CheckoutResultDto> CheckoutAsync(CheckoutRequestDto request)
    {
        try
        {
            _logger.LogInformation("Starting checkout process for customer {CustomerId}", request.CustomerId);

            // Step 1: Get and validate cart
            var cart = await _cartRepository.GetByCustomerIdAsync(request.CustomerId);
            if (cart == null)
                throw new CartNotFoundException(request.CustomerId, true);

            if (cart.IsEmpty)
                throw new InvalidCartOperationException("Cannot checkout an empty cart");

            // Step 2: Initiate checkout (publishes CartCheckoutInitiatedEvent)
            cart.InitiateCheckout();

            // Step 3: Validate inventory availability
            _logger.LogInformation("Validating inventory for {ItemCount} items", cart.Items.Count);
            var inventoryChecks = cart.Items.Select(item => new ProductAvailabilityCheck
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList();

            var availabilityResults = await _inventoryService.CheckProductsAvailabilityAsync(inventoryChecks);
            var unavailableProducts = availabilityResults.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();

            if (unavailableProducts.Any())
            {
                throw new InvalidCartOperationException($"Products not available: {string.Join(", ", unavailableProducts)}");
            }

            // Step 4: Reserve inventory
            _logger.LogInformation("Reserving inventory for checkout");
            var reservationItems = cart.Items.Select(item => new StockReservationItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList();

            var reservationResult = await _inventoryService.ReserveStockAsync(request.CustomerId, reservationItems);
            if (!reservationResult.Success)
            {
                throw new InvalidCartOperationException($"Failed to reserve inventory: {reservationResult.ErrorMessage}");
            }

            // Step 5: Calculate final pricing
            _logger.LogInformation("Calculating final pricing");
            var pricingItems = cart.Items.Select(item => new CartItemForPricing
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                CurrentUnitPrice = item.UnitPrice
            }).ToList();

            var pricingResult = await _pricingService.CalculateCartPricingAsync(request.CustomerId, pricingItems);

            // Step 6: Create order
            _logger.LogInformation("Creating order with total amount {TotalAmount}", pricingResult.TotalAmount);
            var orderRequest = new CreateOrderRequest
            {
                CustomerId = request.CustomerId,
                Items = cart.Items.Select(item => new OrderItemRequest
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = pricingResult.ItemPrices.GetValueOrDefault(item.ProductId, item.UnitPrice),
                    Currency = item.Currency
                }).ToList(),
                Currency = cart.Currency,
                TotalAmount = pricingResult.TotalAmount,
                ShippingAddress = request.ShippingAddress,
                PaymentMethod = request.PaymentMethod,
                AdditionalData = request.AdditionalData
            };

            var orderResult = await _orderService.CreateOrderAsync(orderRequest);
            if (!orderResult.Success || !orderResult.OrderId.HasValue)
            {
                // Release inventory reservation on order creation failure
                if (reservationResult.ReservationId.HasValue)
                {
                    await _inventoryService.ReleaseStockReservationAsync(reservationResult.ReservationId.Value);
                }
                throw new InvalidCartOperationException($"Failed to create order: {orderResult.ErrorMessage}");
            }

            // Step 7: Complete checkout
            cart.CompleteCheckout(orderResult.OrderId.Value);

            // Step 8: Save changes and publish events
            await _cartRepository.UpdateAsync(cart);
            await PublishDomainEventsAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Checkout completed successfully for customer {CustomerId}, order {OrderId}", 
                request.CustomerId, orderResult.OrderId.Value);

            return new CheckoutResultDto
            {
                OrderId = orderResult.OrderId.Value,
                CustomerId = request.CustomerId,
                TotalAmount = pricingResult.TotalAmount,
                Currency = cart.Currency,
                CheckoutCompletedAt = DateTime.UtcNow,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Checkout failed for customer {CustomerId}", request.CustomerId);
            return new CheckoutResultDto
            {
                CustomerId = request.CustomerId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateCartForCheckoutAsync(Guid customerId)
    {
        try
        {
            var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
            
            if (cart == null || cart.IsEmpty)
                return false;

            // Check inventory availability
            var inventoryChecks = cart.Items.Select(item => new ProductAvailabilityCheck
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList();

            var availabilityResults = await _inventoryService.CheckProductsAvailabilityAsync(inventoryChecks);
            var allAvailable = availabilityResults.All(kvp => kvp.Value);

            if (!allAvailable)
            {
                _logger.LogWarning("Cart validation failed for customer {CustomerId}: inventory not available", customerId);
                return false;
            }

            // Additional validation could include:
            // - Validate pricing is current
            // - Check customer eligibility
            // - Validate shipping address
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating cart for customer {CustomerId}", customerId);
            return false;
        }
    }

    private async Task PublishDomainEventsAsync(Domain.Aggregates.CartAggregate cart)
    {
        var events = cart.DomainEvents.ToList();
        cart.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            await _eventBus.PublishAsync(domainEvent);
        }
    }
}