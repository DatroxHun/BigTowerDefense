using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.Universal;

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
    public int HitPoints { get; private set; }
    protected List<Status> Status;
    protected List<DamageType> Vulnerabilities;
    protected List<DamageType> Resistances;
    // Dictionary: dmgtype-multiplier
    protected TimeSpan RestTime; // TimeSpan?
                                 // protected IShield? shield;


    // practical shield representation
    public int MaxShield { get; private set; }
    public int CurrentShield { get; private set; }
    public float ShieldRegenerationSpeed { get; private set; } // what type?



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
