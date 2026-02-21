using UnityEngine;

public class ComponentModule { }

public abstract class Tower : Entity
{
    protected ComponentModule module;

    // protected Behavior b;

    protected Tower( /* Behaviour b */) : base(null)
    { 
        //this.b = b;
    }
}
