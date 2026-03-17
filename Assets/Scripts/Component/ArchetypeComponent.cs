using System;

public abstract class ArchetypeComponent : IDescribable
{
    public IStatProvider StatProvider { get; private set; } // is here to set basic values
    public AttackAction AttackAction { get; private set; }
    public EventHandler<Enemy> OnHit; // Hook for AdvancedTowerComponenets to subscribe
    // come up with other ones
    public void ClearHooks()
    {
        throw new NotImplementedException();
    }
    public abstract string Description(IStatProvider statProvider);
}
