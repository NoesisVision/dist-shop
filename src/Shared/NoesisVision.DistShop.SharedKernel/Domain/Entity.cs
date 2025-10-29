namespace NoesisVision.DistShop.SharedKernel.Domain;

/// <summary>
/// Base class for all domain entities with identity
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }
    
    protected Entity()
    {
        Id = Guid.NewGuid();
    }
    
    protected Entity(Guid id)
    {
        Id = id;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;
            
        if (ReferenceEquals(this, other))
            return true;
            
        if (GetType() != other.GetType())
            return false;
            
        return Id.Equals(other.Id);
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    
    public static bool operator ==(Entity? left, Entity? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}