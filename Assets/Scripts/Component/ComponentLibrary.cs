using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public static class ComponentLibrary
{
    public static TowerComponent RangeUpgrade = new TowerComponent(new StatAlteration(new List<(TowerStats, float, float)> { (TowerStats.TowerRange, 0, 1.2f) }), null, null);

    private static IEnumerator PoisonLogic(Dictionary<TowerStats,float> stats,Enemy enemy)
    {
        var wait = new WaitForSeconds(0.4f);
        while (enemy.IsAlive)
        {
            Debug.Log($"Poisoned for: {stats[TowerStats.PoisonDamage]}");
            enemy.ApplyDamage(new DamageObj() { poison = stats[TowerStats.PoisonDamage] });
            yield return wait;
        }
    }
    public static TowerComponent PoisonComponent = new TowerComponent(new(new() {(TowerStats.PoisonDamage , 5, 1) }),
        new AdvancedAttackAlteration(new List<TowerStats>() { TowerStats.PoisonDamage }, PoisonLogic)
        ,null);
}
