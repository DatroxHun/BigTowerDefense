using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Pool;

// gunner tower
public class LongRangeTower : TargetingTower
{
    [SerializeField]
    private LayerMask obstacleLayer;

    [SerializeField]
    private Bullet bulletPrefab;

    [SerializeField]
    private float Inaccuracy;

    private ObjectPool<Bullet> bulletPool;

    private new void Start()
    {
        bulletPool = new ObjectPool<Bullet>
        (
            createFunc: () =>
            {
                return Instantiate(bulletPrefab);
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
        //Debug.Log($"[GUNNER] : SETUP DONE");
        module.AddComponent(ComponentLibrary.PoisonComponent);
        base.Start();
    }

    private void Shoot(Vector3 targetPoint, Action<Entity> impactEffect)
    {
        Bullet bullet = bulletPool.Get();

        // this could be passed and set in Bullet;
        // might be useful; don't know yet
        // -K
        bullet.transform.position = this.transform.position
            + Vector3.right * (UnityEngine.Random.value * 2 - 1)
            + Vector3.up * (UnityEngine.Random.value * 0.5f);

        bullet.Launch(targetPoint, impactEffect, bulletPool);
    }

    protected override void Action()
    {
        //Debug.Log($"[GUNNER] : TRYING TO SHOOT");
        if (CurrentTarget == null || !CurrentTarget.GetCoordinates().Any())
            return;

        //Debug.Log($"[GUNNER] : SHOOTING");
        var (alteredTarget, alteredNum) = module.GetTargettingAletration()((CurrentTarget, 1));
        IEnumerable<Vector3> closestTargets = alteredTarget
            .GetCoordinates()
            .Take(alteredNum);

        Vector3 preciseTarget = CurrentTarget
            .GetCoordinates()
            .OrderBy(x => UnityEngine.Random.value)
            .First();

        foreach (var target in closestTargets)
        {
            Vector3 perturbance = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 finalTarget = target + perturbance * Inaccuracy;
            DamageObj dmg = AttackDamage;

            List<Func<Enemy, IEnumerator>> effects = new()
        {
            enemy => Effects.InstantDamage(enemy, dmg)
        };

            Shoot(finalTarget,
                (entity) =>
                {
                    var stats = CurrentStats;
                    if (entity is Enemy enemy)
                    {
                        // foreach cr in crlist : StartCoroutine(cr(enemy))
                        foreach (var effect in effects)
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
        }

        //ComponentModule.AugmentTargetChoice(CurrentTarget, ActuallyPrioritizedTarget(=randomTarget))

        // crlist = defaultcr ++ CM.GetCroutines()
    }

    protected override void Target()
    {
        List<Enemy> enemies = EnemyManager.instance.Enemies; 

        List<Entity> targets = enemies
            .Where(t =>
                rangeCollider.OverlapPoint(t.transform.position) &&
                IsDetectable(t.transform.position))
            .Select(e => e as Entity)
            .ToList();

        this.CurrentTarget = new MultiTarget(targets);

        // for testing; delete later
        // - K
        //Debug.Log($"[GUNNER] : ELIGABLE TARGETS: {targets.Count}");
    }


    /// <summary>
    /// Check if line-of-sight from tower to target is unobstructed.
    /// </summary>
    /// <param name="target">Position of target point</param>
    /// <returns>Is the line-of-sight clear?</returns>
    private bool IsDetectable(Vector3 target)
    {
        RaycastHit2D result =
            Physics2D.Linecast(this.transform.position, target, obstacleLayer);
        if (result.collider == null)
        {
            Debug.DrawLine(this.transform.position, target, Color.green, 1.0f);
            return true;
        }
        else
        {
            Debug.DrawLine(this.transform.position, target, Color.red, 1.0f);
            return false;
        }
    }
}
