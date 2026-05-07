using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class BaseTower : TargetingTower
{
    [SerializeField]
    private LayerMask obstacleLayer;

    [SerializeField]
    private Bullet bulletPrefab;

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
            defaultCapacity: 3,
            maxSize: 20
        );

        base.Start();
    }

    private void Shoot(Vector3 targetPoint, Action<Entity> impactEffect)
    {
        Bullet bullet = bulletPool.Get();

        bullet.transform.position = this.transform.position;

        bullet.Launch(targetPoint, impactEffect, bulletPool);
    }

    protected override void Action()
    {
        if (CurrentTarget == null || !CurrentTarget.GetCoordinates().Any())
            return;

        IEnumerable<Vector3> randomTarget = CurrentTarget
            .GetCoordinates()
            .OrderBy(x => UnityEngine.Random.value)
            .Take(1);

        DamageObj dmg = new DamageObj
        {
            physical = 8f,
        };

        List<Func<Enemy, IEnumerator>> effects = new()
        {
            enemy => Effects.InstantDamage(enemy, dmg)
        };

        foreach (var target in randomTarget)
        {
            Shoot(target,
                (entity) =>
                {
                    if (entity is Enemy enemy)
                    {
                        foreach (var effect in effects)
                        {
                            enemy.ApplyEffect(effect(enemy));
                        }
                    }
                }
            );
        }
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
            return true;
        else
            return false;
    }

    protected override void JustDied()
    {
        base.JustDied();

        GameOver.Instance.Toggle();
    }
}
