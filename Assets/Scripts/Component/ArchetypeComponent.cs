using System.Collections.Generic;
using UnityEngine;

public abstract class ArchetypeComponent : IDescribable
{
    public abstract string Description(IStatProvider statProvider);
}
