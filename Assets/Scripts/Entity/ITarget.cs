using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// implement actual target recommendations later
// (could be point, entitiy, collection of either)
// -K

public interface ITarget
{
    List<Vector3> GetCoordinates();
}

public class PointTarget : ITarget
{
    private readonly Vector3 _point;
    public PointTarget(Vector3 point)
    {
        _point = point;
    }
    public List<Vector3> GetCoordinates() => new List<Vector3> { _point };
}

public class MultiTarget : ITarget
{
    private readonly List<ITarget> _targets;
    public MultiTarget(List<ITarget> targets)
    {
        _targets = targets;
    }
    public MultiTarget(List<Entity> entities)
    {
        _targets = entities
            .Select(e =>
                new PointTarget(e.transform.position) as ITarget)
            .ToList();
    }
    public List<Vector3> GetCoordinates() => _targets
        .Select(t => t.GetCoordinates())
        .SelectMany(x => x)
        .ToList();
}

public class EntityTarget : ITarget
{
    public readonly Entity entity;

    public EntityTarget(Entity target)
    {
        this.entity = target;
    }

    public List<Vector3> GetCoordinates()
    {
        return new List<Vector3>() { entity.gameObject.transform.position };
    }
}