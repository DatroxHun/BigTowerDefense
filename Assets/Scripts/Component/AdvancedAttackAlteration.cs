using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class AdvancedAttackAlteration //: IDescribable
{
    public List<TowerStats> Stats { get; private set; }
    public Func<Dictionary<TowerStats, float>,Enemy, IEnumerator> AttackFactory { get; private set; }
    //public AdvancedAttackAlt  Stats = stats; public abstract string Description(Dictionary<TowerStats, float> stats);

    public AdvancedAttackAlteration(List<TowerStats> stats, Func<Dictionary<TowerStats, float>, Enemy, IEnumerator> enumeratorgen)
    {
        Stats = stats;
        AttackFactory = enumeratorgen;
    }
}
