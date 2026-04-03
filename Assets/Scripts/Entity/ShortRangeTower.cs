using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


// rename to Tesla Coil tower or something similar
public class ShortRangeTower : Tower
{

    protected override void Action()
    {
        Debug.Log($"[TESLA COIL] : NO ELECTRICITY");
    }

    protected override void Target()
    {
        List<Enemy> enemies = EnemyManager.instance.Enemies;

        List<Entity> targets = enemies
            .Where(t =>
                rangeCollider.OverlapPoint(t.transform.position))
            .Select(e => e as Entity)
            .ToList();

        this.CurrentTarget = new MultiTarget(targets);

        // for testing; delete later
        // - K
        Debug.Log($"[TESLA COIL] : ELIGABLE TARGETS: {targets.Count}");
        /*
        foreach (var target in targets)
        {
            Enemy e = target as Enemy;
            Debug.Log(e.transform.position);
        }
         */
    }
}
