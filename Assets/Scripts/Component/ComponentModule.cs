using System;
using System.Collections.Generic;
using System.Text;

public abstract class AttackArchetype { }
public abstract class DefendArchetype { }
public abstract class TargetArchetype { } // I can't think of examples right now so this may not be needed
public delegate void AttackAction(ReadOnlySpan<Enemy> targets,IStatProvider stats); // ReadOnlySpan works really well if all of our targetting is physics based
public delegate void FinalAttack(ReadOnlySpan<Enemy> targets); // maybe rename later
public class DefendAction { }
public class TargetAction { }

public abstract class ComponentModule
{
    private ArchetypeComponent _archetypeComponent;
    private List<AdvancedTowerComponent> _advancedComponents;
    private List<BasicTowerComponent> _basicComponents;
    private bool _isAttackDirty;
    private IStatProvider _statProvider;

    public string GenerateDescription() // style later, or maybe instead of putting the string together here we should just return a list of string and let the UI handle the putting together
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(_archetypeComponent.Description(_statProvider));
        foreach (var component in _advancedComponents)
        {
            sb.AppendLine(component.Description(_statProvider));
        }
        return sb.ToString();
    }

    private void RecalculateAttack()
    {
        if (!_isAttackDirty) {  return; }


        // Recreate _statProvider (Archetype then merge Advanceds then modify values based on Basics)
        // Clear previous subscribtions and subscribe all AdvancedComponents to the Archetypes hooks
        _archetypeComponent.ClearHooks();
        foreach (var component in _advancedComponents)
        {
            component.SubscribeHooks(_archetypeComponent);
        }
        _isAttackDirty = false;
    }

    public FinalAttack GetAttack()
    {
        RecalculateAttack();
        return (x => _archetypeComponent.AttackAction(x, _statProvider));
    }
}
