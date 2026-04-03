using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

// gunner tower
public class LongRangeTower : Tower
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
            defaultCapacity: 20,
            maxSize: 100
        );
        Debug.Log($"[GUNNER] : SETUP DONE");

        base.Start();
    }

    private void Shoot(Vector3 targetPoint, Action<Entity> impactEffect)
    {
        Bullet bullet = bulletPool.Get();

        // this could be passed and set in Bullet;
        // might be useful; don't know yet
        // -K
        bullet.transform.position = this.transform.position;

        bullet.Launch(targetPoint, impactEffect, bulletPool);
    }

    protected override void Action()
    {
        Debug.Log($"[GUNNER] : TRYING TO SHOOT");
        if (CurrentTarget == null || !CurrentTarget.GetCoordinates().Any())
            return;

        Debug.Log($"[GUNNER] : SHOOTING");

        Vector3 randomTarget = CurrentTarget
            .GetCoordinates()
            .OrderBy(x => UnityEngine.Random.value)
            .First();

        //ComponentModule.AugmentTargetChoice(CurrentTarget, ActuallyPrioritizedTarget(=randomTarget))

        // crlist = defaultcr ++ CM.GetCroutines()

        rangeCollider.radius += 10;

        Shoot(randomTarget,
            (e) =>
            {
                if (e is Enemy enemy)
                {
                    // foreach cr in crlist : StartCoroutine(cr(enemy))

                    enemy.Return2Pool();
                }
            }
            );
    }

    IEnumerator DefaultDamage(Enemy enemy)
    {
        //                           V--- get actual DMGObject that gets updated upon component updates
        // enemy.applydamage(new DMGOBJ(physical, 10))
        // aaaaaaaaaaaaaaa
        yield return null;
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
        Debug.Log($"[GUNNER] : ELIGABLE TARGETS: {targets.Count}");
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
