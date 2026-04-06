using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicTowerComponent
{
    // these only modify stats so they will have some kind of collection of modifications that will be used to update the IAmplificationProvider
    public List<(TowerStats,float,float)> Modifications { get; private set; }
}
