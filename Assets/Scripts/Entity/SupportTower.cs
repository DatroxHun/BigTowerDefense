using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SupportTower : Tower
{
    new void Start()
    {
        base.Start();
    }

    void Update()
    {
            
    }
    protected override void Action()
    {
        Debug.Log($"[SUPPORT] : DISCHARGING");

        List<Tower> towers =
            TowerManager.instance.Towers
            .Where(e =>
                rangeCollider.OverlapPoint(e.transform.position))
            .ToList();

        List<Enemy> enemies =
            EnemyManager.instance.Enemies
            .Where(e =>
                rangeCollider.OverlapPoint(e.transform.position))
            .ToList();

        List<Func<Tower, IEnumerator>> towerEffects = new()
        {
            tower => Effects.InstantHeal(tower, 10)
        };

        List<Func<Enemy, IEnumerator>> enemyEffects = new()
        {
            
        };

        foreach (Tower tower in towers)
        {
            foreach (var effect in towerEffects)
            {
                tower.ApplyEffect(effect(tower));
            }
        }

        foreach (Enemy enemy in enemies)
        {
            foreach (var effect in enemyEffects)
            {
                enemy.ApplyEffect(effect(enemy));
            }
        }
    }

    protected override void Target()
    {
        // no targeting
    }


    
}
