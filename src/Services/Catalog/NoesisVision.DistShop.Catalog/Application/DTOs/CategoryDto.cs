namespace NoesisVision.DistShop.Catalog.Application.DTOs;

/// <summary>
/// Data transfer object for Category information
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsRootCategory { get; set; }
    public bool HasChildren { get; set; }
}

/// <summary>
/// Data transfer object for creating a new category
/// </summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
}

/// <summary>
/// Data transfer object for updating a category
/// </summary>
public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
}