using NoesisVision.DistShop.SharedKernel.Domain;
using NoesisVision.DistShop.Catalog.Domain.Exceptions;
using NoesisVision.DistShop.Catalog.Contracts.Events;

namespace NoesisVision.DistShop.Catalog.Domain.Aggregates;

/// <summary>
/// Category aggregate root managing hierarchical product organization
/// </summary>
public class CategoryAggregate : AggregateRoot
{
    private string _name;
    private string _description;
    private Guid? _parentCategoryId;
    private bool _isActive;
    private DateTime _createdAt;
    private DateTime _updatedAt;
    private readonly List<Guid> _childCategoryIds;

    // EF Core constructor
    private CategoryAggregate() : base() 
    {
        _name = null!;
        _description = null!;
        _childCategoryIds = new List<Guid>();
    }

    private CategoryAggregate(
        string name,
        string description,
        Guid? parentCategoryId = null) : base()
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _description = description ?? throw new ArgumentNullException(nameof(description));
        _parentCategoryId = parentCategoryId;
        _isActive = true;
        _createdAt = DateTime.UtcNow;
        _updatedAt = DateTime.UtcNow;
        _childCategoryIds = new List<Guid>();

        AddDomainEvent(new CategoryCreatedEvent(
            Id,
            _name,
            _description,
            _parentCategoryId));
    }

    public static CategoryAggregate Create(
        string name,
        string description,
        Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCategoryOperationException("Category name cannot be null or empty");

        if (name.Length > 100)
            throw new InvalidCategoryOperationException("Category name cannot exceed 100 characters");

        return new CategoryAggregate(name.Trim(), description?.Trim() ?? string.Empty, parentCategoryId);
    }

    // Properties
    public string Name => _name;
    public string Description => _description;
    public Guid? ParentCategoryId => _parentCategoryId;
    public bool IsActive => _isActive;
    public DateTime CreatedAt => _createdAt;
    public DateTime UpdatedAt => _updatedAt;
    public IReadOnlyList<Guid> ChildCategoryIds => _childCategoryIds.AsReadOnly();
    public bool IsRootCategory => _parentCategoryId == null;
    public bool HasChildren => _childCategoryIds.Any();

    // Business methods
    public void UpdateDetails(string name, string description)
    {
        if (!_isActive)
            throw new InvalidCategoryOperationException("Cannot update inactive category");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCategoryOperationException("Category name cannot be null or empty");

        if (name.Length > 100)
            throw new InvalidCategoryOperationException("Category name cannot exceed 100 characters");

        var trimmedName = name.Trim();
        var trimmedDescription = description?.Trim() ?? string.Empty;

        if (_name != trimmedName || _description != trimmedDescription)
        {
            _name = trimmedName;
            _description = trimmedDescription;
            _updatedAt = DateTime.UtcNow;

            AddDomainEvent(new CategoryUpdatedEvent(
                Id,
                _name,
                _description,
                _parentCategoryId));
        }
    }

    public void ChangeParent(Guid? newParentCategoryId, IEnumerable<Guid> ancestorIds)
    {
        if (!_isActive)
            throw new InvalidCategoryOperationException("Cannot change parent of inactive category");

        // Prevent setting self as parent
        if (newParentCategoryId == Id)
            throw new CircularCategoryHierarchyException(Id, newParentCategoryId.Value);

        // Prevent circular hierarchy by checking if new parent is a descendant
        if (newParentCategoryId.HasValue && ancestorIds.Contains(newParentCategoryId.Value))
            throw new CircularCategoryHierarchyException(Id, newParentCategoryId.Value);

        if (_parentCategoryId != newParentCategoryId)
        {
            _parentCategoryId = newParentCategoryId;
            _updatedAt = DateTime.UtcNow;

            AddDomainEvent(new CategoryUpdatedEvent(
                Id,
                _name,
                _description,
                _parentCategoryId));
        }
    }

    public void AddChildCategory(Guid childCategoryId)
    {
        if (!_isActive)
            throw new InvalidCategoryOperationException("Cannot add child to inactive category");

        if (childCategoryId == Id)
            throw new InvalidCategoryOperationException("Category cannot be its own child");

        if (!_childCategoryIds.Contains(childCategoryId))
        {
            _childCategoryIds.Add(childCategoryId);
            _updatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveChildCategory(Guid childCategoryId)
    {
        if (_childCategoryIds.Remove(childCategoryId))
        {
            _updatedAt = DateTime.UtcNow;
        }
    }

    public void Activate()
    {
        if (_isActive)
            return;

        _isActive = true;
        _updatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!_isActive)
            return;

        if (HasChildren)
            throw new InvalidCategoryOperationException("Cannot deactivate category that has child categories");

        _isActive = false;
        _updatedAt = DateTime.UtcNow;
    }

    public int GetDepthLevel(Dictionary<Guid, CategoryAggregate> allCategories)
    {
        if (IsRootCategory)
            return 0;

        var depth = 0;
        var currentParentId = _parentCategoryId;

        while (currentParentId.HasValue)
        {
            depth++;
            if (allCategories.TryGetValue(currentParentId.Value, out var parentCategory))
            {
                currentParentId = parentCategory.ParentCategoryId;
            }
            else
            {
                break;
            }

            // Prevent infinite loops in case of data corruption
            if (depth > 10)
                throw new InvalidCategoryOperationException("Category hierarchy is too deep or contains cycles");
        }

        return depth;
    }
}