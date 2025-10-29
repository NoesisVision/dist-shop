using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.OnlineShopping.Application.DTOs;
using NoesisVision.DistShop.OnlineShopping.Domain.Aggregates;
using NoesisVision.DistShop.OnlineShopping.Domain.Repositories;
using NoesisVision.DistShop.OnlineShopping.Domain.Exceptions;

namespace NoesisVision.DistShop.OnlineShopping.Application.Services;

/// <summary>
/// Application service for cart operations
/// </summary>
public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IPricingIntegrationService _pricingService;
    private readonly IInventoryIntegrationService _inventoryService;

    public CartService(
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IPricingIntegrationService pricingService,
        IInventoryIntegrationService inventoryService)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
    }

    public async Task<CartDto?> GetCartByCustomerIdAsync(Guid customerId)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
        return cart != null ? MapToDto(cart) : null;
    }

    public async Task<CartDto> GetOrCreateCartAsync(Guid customerId, string currency = "USD")
    {
        var cart = await _cartRepository.GetOrCreateByCustomerIdAsync(customerId, currency);
        
        // Publish events if any
        await PublishDomainEventsAsync(cart);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(Guid customerId, AddToCartRequestDto request)
    {
        // Validate inventory availability before adding to cart
        var isAvailable = await _inventoryService.IsProductAvailableAsync(request.ProductId, request.Quantity);
        if (!isAvailable)
        {
            throw new InvalidCartOperationException($"Product {request.ProductId} is not available in the requested quantity");
        }

        // Get current pricing
        var currentPrice = await _pricingService.GetProductPriceAsync(request.ProductId, customerId, request.Quantity);

        var cart = await _cartRepository.GetOrCreateByCustomerIdAsync(customerId, request.Currency);
        
        cart.AddProduct(
            request.ProductId,
            request.ProductName,
            request.Quantity,
            currentPrice); // Use current price from pricing service

        await _cartRepository.UpdateAsync(cart);
        await PublishDomainEventsAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(cart);
    }

    public async Task<CartDto> RemoveFromCartAsync(Guid customerId, Guid productId)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
        if (cart == null)
            throw new CartNotFoundException(customerId, true);

        cart.RemoveProduct(productId);

        await _cartRepository.UpdateAsync(cart);
        await PublishDomainEventsAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(Guid customerId, Guid productId, UpdateCartItemRequestDto request)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
        if (cart == null)
            throw new CartNotFoundException(customerId, true);

        cart.UpdateProductQuantity(productId, request.Quantity);

        await _cartRepository.UpdateAsync(cart);
        await PublishDomainEventsAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(cart);
    }

    public async Task<CartDto> ClearCartAsync(Guid customerId)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
        if (cart == null)
            throw new CartNotFoundException(customerId, true);

        cart.Clear();

        await _cartRepository.UpdateAsync(cart);
        await PublishDomainEventsAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(cart);
    }

    public async Task<CartDto> RefreshCartPricesAsync(Guid customerId)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId);
        if (cart == null)
            throw new CartNotFoundException(customerId, true);

        // Get current prices from Pricing Service
        var productIds = cart.Items.Select(item => item.ProductId).ToList();
        var currentPrices = await _pricingService.GetProductPricesAsync(productIds, customerId);

        // Update cart item prices
        var hasChanges = false;
        foreach (var item in cart.Items.ToList())
        {
            if (currentPrices.TryGetValue(item.ProductId, out var newPrice) && newPrice != item.UnitPrice)
            {
                cart.UpdateProductPrice(item.ProductId, newPrice);
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await _cartRepository.UpdateAsync(cart);
            await PublishDomainEventsAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        return MapToDto(cart);
    }

    private async Task PublishDomainEventsAsync(CartAggregate cart)
    {
        var events = cart.DomainEvents.ToList();
        cart.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            await _eventBus.PublishAsync(domainEvent);
        }
    }

    private static CartDto MapToDto(CartAggregate cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Currency = cart.Currency,
            Items = cart.Items.Select(MapItemToDto).ToList(),
            ItemCount = cart.ItemCount,
            TotalQuantity = cart.TotalQuantity,
            TotalAmount = cart.TotalAmount,
            IsEmpty = cart.IsEmpty,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            LastActivityAt = cart.LastActivityAt
        };
    }

    private static CartItemDto MapItemToDto(Domain.ValueObjects.CartItem item)
    {
        return new CartItemDto
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Currency = item.Currency,
            TotalPrice = item.TotalPrice,
            AddedAt = item.AddedAt
        };
    }
}