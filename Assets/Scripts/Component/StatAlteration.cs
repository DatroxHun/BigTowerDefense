using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StatAlteration
{
    public List<(TowerStats,float,float)> Modifications { get; private set; }
    public StatAlteration(List<(TowerStats, float, float)> modifications)
    {
        Modifications = modifications;
    }
}
