using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.Inventory.Domain.Aggregates;
using NoesisVision.DistShop.Inventory.Domain.Repositories;
using NoesisVision.DistShop.Inventory.Domain.ValueObjects;
using NoesisVision.DistShop.Inventory.Domain.Exceptions;

namespace NoesisVision.DistShop.Inventory.Application.Services;

/// <summary>
/// Application service for inventory operations
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public async Task<InventoryItem> CreateInventoryItemAsync(Guid productId, int initialQuantity, int reorderLevel, int maxStockLevel)
    {
        // Check if inventory item already exists for this product
        var existingItem = await _inventoryRepository.GetByProductIdAsync(productId);
        if (existingItem != null)
        {
            throw new InventoryDomainException($"Inventory item already exists for product {productId}");
        }

        var inventoryItem = new InventoryItem(productId, initialQuantity, reorderLevel, maxStockLevel);
        
        await _inventoryRepository.AddAsync(inventoryItem);
        
        // Publish domain events
        await PublishDomainEventsAsync(inventoryItem);
        
        await _unitOfWork.SaveChangesAsync();
        
        return inventoryItem;
    }

    public async Task<InventoryItem?> GetInventoryByProductIdAsync(Guid productId)
    {
        return await _inventoryRepository.GetByProductIdAsync(productId);
    }

    public async Task<StockReservation> ReserveStockAsync(Guid productId, int quantity, TimeSpan duration, string? reference = null)
    {
        var inventoryItem = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventoryItem == null)
        {
            throw new InventoryDomainException($"Inventory item not found for product {productId}");
        }

        var reservation = inventoryItem.ReserveStock(quantity, duration, reference);
        
        await _inventoryRepository.UpdateAsync(inventoryItem);
        
        // Publish domain events
        await PublishDomainEventsAsync(inventoryItem);
        
        await _unitOfWork.SaveChangesAsync();
        
        return reservation;
    }

    public async Task ReleaseReservationAsync(Guid productId, Guid reservationId, string reason = "Manual release")
    {
        var inventoryItem = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventoryItem == null)
        {
            throw new InventoryDomainException($"Inventory item not found for product {productId}");
        }

        inventoryItem.ReleaseReservation(reservationId, reason);
        
        await _inventoryRepository.UpdateAsync(inventoryItem);
        
        // Publish domain events
        await PublishDomainEventsAsync(inventoryItem);
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ConfirmReservationAsync(Guid productId, Guid reservationId)
    {
        var inventoryItem = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventoryItem == null)
        {
            throw new InventoryDomainException($"Inventory item not found for product {productId}");
        }

        inventoryItem.ConfirmReservation(reservationId);
        
        await _inventoryRepository.UpdateAsync(inventoryItem);
        
        // Publish domain events
        await PublishDomainEventsAsync(inventoryItem);
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AdjustStockAsync(Guid productId, int quantityChange, string reason)
    {
        var inventoryItem = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventoryItem == null)
        {
            throw new InventoryDomainException($"Inventory item not found for product {productId}");
        }

        inventoryItem.AdjustStock(quantityChange, reason);
        
        await _inventoryRepository.UpdateAsync(inventoryItem);
        
        // Publish domain events
        await PublishDomainEventsAsync(inventoryItem);
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync()
    {
        return await _inventoryRepository.GetLowStockItemsAsync();
    }

    public async Task UpdateStockLevelsAsync(Guid productId, int reorderLevel, int maxStockLevel)
    {
        var inventoryItem = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventoryItem == null)
        {
            throw new InventoryDomainException($"Inventory item not found for product {productId}");
        }

        inventoryItem.UpdateStockLevels(reorderLevel, maxStockLevel);
        
        await _inventoryRepository.UpdateAsync(inventoryItem);
        
        // Publish domain events
        await PublishDomainEventsAsync(inventoryItem);
        
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Publishes all domain events from the aggregate and clears them
    /// </summary>
    /// <param name="aggregate">The aggregate with domain events</param>
    private async Task PublishDomainEventsAsync(InventoryItem aggregate)
    {
        var domainEvents = aggregate.DomainEvents.ToList();
        aggregate.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            await _eventBus.PublishAsync(domainEvent);
        }
    }
}