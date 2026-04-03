#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    [SerializeField]
    protected CircleCollider2D rangeCollider = null!;

    [field: SerializeField]
    public float MaxHitPoints { get; protected set; }

    [field: SerializeField]
    private DamageObj Vulnerabilities { get; set; } = DamageObj.One;

    public float HitPoints { get; protected set; }
    public bool IsAlive => HitPoints > 0;


    // all entitites have targets, even support class ones
    public ITarget? CurrentTarget { get; protected set; }

    // practical shield representation
    public float MaxShield { get; protected set; }
    public float CurrentShield { get; protected set; }
    public float ShieldRegenerationSpeed { get; protected set; } // what type?
    public List<Vector3> GetCoordinates() => new List<Vector3> { transform.position };

    private List<Coroutine> statusEffects = new List<Coroutine>();

    protected abstract void Action();
    protected abstract void Target();

    public Coroutine ApplyEffect(IEnumerator effect) 
    {
        Coroutine coroutine = StartCoroutine(effect);
        statusEffects.Add(coroutine);
        return coroutine;
    }

    protected void ClearAllEffect()
    {
        foreach (Coroutine effect in statusEffects)
        {
            if (effect != null) StopCoroutine(effect);
        }
    }

    public void ApplyDamage(DamageObj dobj)
    {
        DamageObj finalDmg = dobj * Vulnerabilities;

        HitPoints -= finalDmg.direct;
        HitPoints -= finalDmg.physical;
        HitPoints -= finalDmg.fire;
        HitPoints -= finalDmg.electric;
        HitPoints -= finalDmg.poison;

        Debug.Log(HitPoints);

        if (!IsAlive) JustDied();
    }

    protected virtual void JustDied()
    {
        Debug.Log("Just Died");
        ClearAllEffect();
    }
}

[System.Serializable]
public class DamageObj
{
    public float direct;
    public float physical;
    public float fire;
    public float electric;
    public float poison;

    public static DamageObj operator +(DamageObj a, DamageObj b)
    {
        DamageObj result = new DamageObj();
        result.direct = a.direct + b.direct;
        result.physical = a.physical + b.physical;
        result.fire = a.fire + b.fire;
        result.electric = a.electric + b.electric;
        result.poison = a.poison + b.poison;

        return result;
    }

    public static DamageObj operator *(DamageObj a, DamageObj b)
    {
        DamageObj result = new DamageObj();
        result.direct = a.direct * b.direct;
        result.physical = a.physical * b.physical;
        result.fire = a.fire * b.fire;
        result.electric = a.electric * b.electric;
        result.poison = a.poison * b.poison;

        return result;
    }

    public static DamageObj One
    {
        get
        {
            DamageObj result = new DamageObj();
            result.direct = result.physical = 1;
            result.fire = result.electric = result.poison = 1;
            return result;
        }
    } 
}