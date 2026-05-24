#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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
    [SerializeField] protected Slider healthbar = null!;

    [SerializeField] protected CircleCollider2D rangeCollider = null!;
    public virtual float MaxHitPoints { get => maxHitPoints; protected set => maxHitPoints = value; }

    [field: SerializeField] protected virtual DamageObj Vulnerabilities { get; set; } = DamageObj.One;
    [field: SerializeField] protected virtual DamageObj AttackDamage { get; set; }

    protected virtual bool Invulnerable() => false;

    private float hitpoints;
    public float HitPoints
    { 
        get => hitpoints; 
        protected set
        {
            hitpoints = Invulnerable() ? hitpoints : Mathf.Min(value, MaxHitPoints);

            if (healthbar != null)
                healthbar.value = HitPoints / MaxHitPoints;
        }
    }

    public bool IsAlive => HitPoints > 1e-3f;


    // all entitites have targets, even support class ones
    public ITarget? CurrentTarget { get; protected set; }

    // practical shield representation
    public virtual float MaxShield { get => maxShield; protected set => maxShield = value; }
    public float CurrentShield { get; protected set; }
    public virtual float ShieldRegenerationSpeed { get => shieldRegenerationSpeed; protected set => shieldRegenerationSpeed = value; } // what type?
    public List<Vector3> GetCoordinates() => new List<Vector3> { transform.position };

    private List<Coroutine> statusEffects = new List<Coroutine>();
    [SerializeField]
    protected float maxHitPoints;
    protected float shieldRegenerationSpeed;
    protected float maxShield;

    protected abstract void Action();

    protected void Start()
    {
        HitPoints = MaxHitPoints;
    }

    public Coroutine ApplyEffect(IEnumerator effect) 
    {
        if(!IsAlive) { return null; }
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

    public virtual void ApplyDamage(DamageObj dobj)
    {
        /*if (Invulnerable())
            return; */

        DamageObj finalDmg = dobj * Vulnerabilities;

        HitPoints -= finalDmg.direct;
        HitPoints -= finalDmg.physical;
        HitPoints -= finalDmg.fire;
        HitPoints -= finalDmg.electric;
        HitPoints -= finalDmg.poison;

        if (!IsAlive) JustDied();
    }

    public void ApplyHeal(float healedHP)
    {
        if (IsAlive)
        {
            if (HitPoints != MaxHitPoints)
            {
                ParticlePool.Emit(transform.position, ParticleType.Divine);
            }

            // clamping handled in HitPoints setter
            HitPoints += healedHP;
        }
    }

    protected virtual void JustDied()
    {
        Debug.Log("Just Died");
        ClearAllEffect();
    }
}

[System.Serializable]
public struct DamageObj
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