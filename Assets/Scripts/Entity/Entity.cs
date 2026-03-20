using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Status { }

public class DamageType { }


// OOP hell
/*
public interface IShield
{    
    int GetMaxShield();
    int GetCurrentShield();

    //blabla
}
*/
/*
public class Shield : IShield
{
    int regentime;
    ...
}
*/

// implement actual target recommendations later
// (could be point, entitiy, collection of either)
// -K
public interface ITarget
{
    List<Vector3> GetCoordinates();
}

public class  PointTarget : ITarget
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
    public List<Vector3> GetCoordinates() => _targets
        .Select(t => t.GetCoordinates())
        .SelectMany(x => x)
        .ToList();
}


public abstract class Entity : MonoBehaviour, ITarget
{


    // exposed in inspector
    [SerializeField]
    private int maxHitPoints;
    public int MaxHitPoints
    {
        get { return maxHitPoints; }
        private set { maxHitPoints = value; }
    }

    public int HitPoints { get; private set; }
    public List<Status> Status { get; private set; }
    public List<DamageType> Vulnerabilities { get; private set; }
    public List<DamageType> Resistances { get; private set; }
    // Dictionary: dmgtype-multiplier


    /// <summary>
    /// Time between (re)targeting attempts
    /// </summary>
    [SerializeField]
    protected float retargetDelaySeconds = 10.0f;

    /// <summary>
    /// Time it takes to choose a target
    /// </summary>
    [SerializeField]
    protected float targetTimeSeconds = 2.0f;


    [SerializeField]
    protected float actiondelaySeconds = 2.0f;

    // all entitites have targets, even support class ones
    public ITarget CurrentTarget { get; private set; }

    // practical shield representation
    public int MaxShield { get; private set; }
    public int CurrentShield { get; private set; }
    public float ShieldRegenerationSpeed { get; private set; } // what type?
    public List<Vector3> GetCoordinates() => new List<Vector3> { transform.position };



    // OOP hell
    // public bool Shielded() => shield is not null;
    // public int GetMaxShield() => shield?.GetMaxShield() ?? 0;
    // public int GetCurretnShield => shield?.GetCurrentShield() ?? 0;
    
    /*
    protected Entity(IShield? shield)
    {
        this.shield = shield;
    }
    */

    protected abstract void Action();
    protected abstract void Target();


}
