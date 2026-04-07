using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedTargettingAlteration // IDescribable later
{
    public Func<List<Entity>, List<Entity>, List<Entity>> RePrioritize { get; private set; } // Eiligible Targets , Current Targets -> Current Targets
                                                                        // Order should up to down and left to right based on the placemnets of the component
    public AdvancedTargettingAlteration(Func<List<Entity>, List<Entity>, List<Entity>> func)
    {
        RePrioritize = func;
    }
}
