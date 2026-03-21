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
                Vector3.Distance(t.transform.position, this.transform.position) <= this.range)
                .Select(e => e as ITarget).ToList();

        this.CurrentTarget = new MultiTarget(targets);

        // for testing; delete later
        // - K
        foreach (var target in targets)
        {
            Enemy e = target as Enemy;
            Debug.Log(e.transform.position);
        }
    }
}
