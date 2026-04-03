#nullable enable

using System;
using System.Collections;
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

public abstract class Entity : MonoBehaviour
{
    [field: SerializeField]
    public int MaxHitPoints { get; private set; }

    [SerializeField]
    protected CircleCollider2D rangeCollider;

    public int HitPoints { get; private set; }
    public bool IsAlive => HitPoints > 0;
    public List<Status> Status { get; private set; }
    public List<DamageType> Vulnerabilities { get; private set; }
    public List<DamageType> Resistances { get; private set; }
    // Dictionary: dmgtype-multiplier


    // all entitites have targets, even support class ones
    public ITarget? CurrentTarget { get; protected set; }

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

    public Coroutine ApplyEffect(IEnumerator effect) 
    {
        return StartCoroutine(effect);
    }

    protected void ClearAllEffect()
    {

    }


    // should be written uniformly for all entities (use vulnerabilities and resistances)
    public void ApplyDamage(DamageObj dobj)
    {
        // DO STUFF

        if (!IsAlive) JustDied();
    }

    protected abstract void JustDied();
}

public class DamageObj
{
    public float direct;
    public float physical;
    public float fire;
    public float electric;
    public float poison;
}