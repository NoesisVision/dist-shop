using NoesisVision.DistShop.SharedKernel.Domain;

namespace NoesisVision.DistShop.Catalog.Domain.Exceptions;

/// <summary>
/// Base exception for catalog domain-specific errors
/// </summary>
public abstract class CatalogDomainException : DomainException
{
    protected CatalogDomainException(string message) : base(message) { }
    protected CatalogDomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a product operation is invalid
/// </summary>
public class InvalidProductOperationException : CatalogDomainException
{
    public InvalidProductOperationException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a category operation is invalid
/// </summary>
public class InvalidCategoryOperationException : CatalogDomainException
{
    public InvalidCategoryOperationException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when trying to create a circular category hierarchy
/// </summary>
public class CircularCategoryHierarchyException : CatalogDomainException
{
    public CircularCategoryHierarchyException(Guid categoryId, Guid parentId) 
        : base($"Setting category {parentId} as parent of {categoryId} would create a circular hierarchy") { }
}