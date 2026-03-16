using System.Collections.Generic;
using System.Data;
using UnityEngine;

public interface IDescribable
{
    public string Description(IStatProvider statProvider);
}
