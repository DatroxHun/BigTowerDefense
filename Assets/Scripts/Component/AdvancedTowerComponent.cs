using System.Collections.Generic;
using UnityEngine;

public abstract class AdvancedTowerComponent : IDescribable // this will probably be the same abstract class as ArchetypeComponent
{
    public IStatProvider StatProvider { get; private set; }
    public abstract void SubscribeHooks(ArchetypeComponent component);
    public abstract string Description(IStatProvider statProvider);
}
