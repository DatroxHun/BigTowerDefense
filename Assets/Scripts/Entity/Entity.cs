using System;
using System.Collections.Generic;

using UnityEngine;

public class Status { }

public class DamageType { }

public interface IShield
{    
    int GetMaxShield();
    int GetCurrentShield();

    //blabla
}
public abstract class Entity
{
    protected int HitPoints;
    protected List<Status> Status;
    protected List<DamageType> Vulnerabilities;
    protected List<DamageType> Resistances;
    protected TimeSpan RestTime; // TimeSpan?
    protected IShield? shield;


    public bool Shielded() => shield is not null;
    public int GetMaxShield() => shield?.GetMaxShield() ?? 0;
    public int GetCurretnShield => shield?.GetCurrentShield() ?? 0;


    protected Entity(IShield? shield)
    {
        this.shield = shield;
    }


    protected abstract void Action();
    protected abstract void Target();


}
