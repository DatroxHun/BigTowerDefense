using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StatAlteration
{
    // these only modify stats so they will have some kind of collection of modifications that will be used to update the IAmplificationProvider
    public List<(TowerStats,float,float)> Modifications { get; private set; }
    public StatAlteration(List<(TowerStats, float, float)> modifications)
    {
        Modifications = modifications;
    }
}
