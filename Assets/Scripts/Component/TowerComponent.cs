using UnityEngine;
#nullable enable
public class TowerComponent
{
    public StatAlteration? StatAlteration { get; private set; } // they are null if there is none
    public AdvancedAttackAlteration? AdvancedAttackAlteration { get; private set; }
    public AdvancedTargettingAlteration? AdvancedTargettingAlteration { get; private set; }

    public TowerComponent(StatAlteration? statAlteration, AdvancedAttackAlteration? advancedAttackAlteration, AdvancedTargettingAlteration? advancedTargettingAlteration)
    {
        StatAlteration = statAlteration;
        AdvancedAttackAlteration = advancedAttackAlteration;
        AdvancedTargettingAlteration = advancedTargettingAlteration;
    }
}
