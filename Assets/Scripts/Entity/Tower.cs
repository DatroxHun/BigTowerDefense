using UnityEngine;

public class ComponentModule { }

public abstract class Tower : Entity
{
    protected ComponentModule module;

    // protected Behavior b;

    private void Start()
    {
        // nothing to put here yet
    }

    private void Update()
    {
        Target();
        Action();
    }
}
