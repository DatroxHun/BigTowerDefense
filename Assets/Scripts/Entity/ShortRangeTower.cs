using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;


// rename to Tesla Coil tower or something similar
public class ShortRangeTower : Tower
{
    [SerializeField]
    private LightningBolt boltPrefab;

    private ObjectPool<LightningBolt> boltPool;

    private new void Start()
    {
        boltPool = new ObjectPool<LightningBolt>
        (
            createFunc: () =>
            {
                return Instantiate(boltPrefab);
            },

            actionOnGet: (obj) =>
            {
                obj.Object.SetActive(true);
            },

            actionOnRelease: (obj) =>
            {
                obj.Object.SetActive(false);
            },

            actionOnDestroy: (obj) =>
            {
                Destroy(obj.Object);
            },
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 100
        );
        Debug.Log($"[TESLA COIL] : SETUP DONE");

        base.Start();
    }
    protected override void Action()
    {
        if (CurrentTarget == null || !CurrentTarget.GetCoordinates().Any())
            return;

        Debug.Log($"[TESLA COIL] : DISCHARGING");

        IEnumerable<Vector3> closestTargets = CurrentTarget
            .GetCoordinates()
            .OrderBy(x => Vector3.Distance(transform.position, x))
            .Take(3);

        rangeCollider.radius += 10;

        foreach (Vector3 target in closestTargets)
        {
            Shoot(target,
            (entity) =>
               {
                   if (entity is Enemy enemy)
                    {
                        // foreach cr in crlist : StartCoroutine(cr(enemy))

                        
                    }
                }
            );
        }

        CurrentTarget = null;

    }

    private void Shoot(Vector3 targetPoint, Action<Entity> impactEffect)
    {
        LightningBolt bolt = boltPool.Get();

        bolt.Launch(transform.position, targetPoint, impactEffect, boltPool);
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
