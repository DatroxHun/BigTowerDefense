using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// rename to Tesla Coil tower or something similar
public class ShortRangeTower : Tower
{

    protected override void Action()
    {
        throw new System.NotImplementedException();
    }

    protected override void Target()
    {
        List<Enemy> enemies = EnemyManager.instance.Enemies;

        

        List<ITarget> targets = enemies
            .Where(t =>
                rangeCollider.OverlapPoint(t.transform.position))
                .Select(e => e as ITarget).ToList();

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
