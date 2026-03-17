using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class StatProvider : IStatProvider
{
    private Dictionary<TowerStats, (float basestat, float multiplier)> Stats; // are these actually the right thing to store?

    public float? GetStatValue(TowerStats statName)
    {
        return Stats.TryGetValue(statName, out (float, float) value) ? value.Item1 * value.Item2 : null;
    }

    public void Merge(IStatProvider statstomerge)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateStat(TowerStats statName, float basemodification, float multipliermodification) 
    {
        throw new System.NotImplementedException();
    }
}
