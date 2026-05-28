using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedTargettingAlteration // IDescribable later
{
    public Func<(ITarget, int), (ITarget,int)> RePrioritize { get; private set; } // Eiligible Targets , Current Targets -> Current Targets
                                                                        // Order should up to down and left to right based on the placemnets of the component
    public AdvancedTargettingAlteration(Func<(ITarget, int), (ITarget, int)> func) // ITarget -> [Enemy] , 
    {
        RePrioritize = func;
    }
}
