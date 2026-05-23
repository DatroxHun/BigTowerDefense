using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Pool;


// rename to Tesla Coil tower or something similar
public class ShortRangeTower : TargetingTower
{
    [SerializeField]
    private LightningBolt boltPrefab;

    private ObjectPool<LightningBolt> boltPool;

    [SerializeField]
    private Vector3 EmissionPointOffset;

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
        module.AddComponent(ComponentLibrary.RangeUpgrade);
        module.AddComponent(ComponentLibrary.RangeUpgrade);

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

        DamageObj dmg = AttackDamage;

        List<Func<Enemy, IEnumerator>> effects = new()
        {
            enemy => Effects.InstantDamage(enemy, dmg)
        };

        StartCoroutine(DischargeAll(closestTargets, effects));
    }

    private IEnumerator DischargeAll(IEnumerable<Vector3> targets, List<Func<Enemy, IEnumerator>> effects)
    {
        foreach (Vector3 target in targets)
        {
            var stats = CurrentStats;
            Shoot(target,
            (entity) =>
            {
                if (entity is Enemy enemy)
                {
                    foreach (Func<Enemy, IEnumerator> effect in effects)
                    {
                        enemy.ApplyEffect(effect(enemy));
                    }
                    foreach (var effect in module.GetAttackAlteration())
                    {
                        enemy.ApplyEffect(effect(stats, enemy));
                    }

                }
            }
            );
            yield return new WaitForSeconds(0.1f);
        }

        CurrentTarget = null;
    }

    private void Shoot(Vector3 targetPoint, Action<Entity> impactEffect)
    {
        LightningBolt bolt = boltPool.Get();

        Vector3 offsetPerturb = (UnityEngine.Random.insideUnitCircle * 0.2f);

        bolt.Launch
        (
            transform.position + EmissionPointOffset + offsetPerturb,
            targetPoint,
            impactEffect,
            boltPool
        );
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
