using System.Collections.Generic;
using UnityEngine;

public abstract class AdvancedTowerComponent : IDescribable // this will probably be the same abstract class as ArchetypeComponent
{
    public List<TowerStats> Stats;
    public abstract string Description(Dictionary<TowerStats, float> stats);
}
