using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class Cannoneer : Enemy
{
    // projectile setup similar to that of GunnerTower

    [SerializeField]
    private LayerMask obstacleLayer;

    [SerializeField]
    private Bullet cannonBallPrefab;

    [SerializeField]
    private float inaccuracy;

    private ObjectPool<Bullet> cannonBallPool;

    

    private new void Start()
    {
        cannonBallPool = new ObjectPool<Bullet>
        (
            createFunc: () =>
            {
                return Instantiate(cannonBallPrefab);
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

        // set in inspector
        //attackRange = 3.5f;

        base.Start();
    }

    protected override void Action()
    {
        Debug.Log("Enemy Action");

        if (CurrentTarget is EntityTarget target && target.entity != null)
        {
            DamageObj dmg = AttackDamage;

            List<Func<Tower, IEnumerator>> effects = new()
            {
                tower => Effects.InstantDamage(tower, dmg)
            };

            // might change later
            Shoot(
                target.entity.transform.position,
                (entity) =>
                {
                    if (entity is Tower tower)
                    {
                        foreach (var effect in effects)
                        {
                            tower.ApplyEffect(effect(tower));
                        }
                    }
                }
            );
        }
    }

    protected void Shoot(Vector3 targetPoint, Action<Entity> impactEffect)
    {
        Bullet bullet = cannonBallPool.Get();

        // this could be passed and set in Bullet;
        // might be useful; don't know yet
        // -K
        bullet.transform.position = this.transform.position;
        bullet.Launch(targetPoint, impactEffect, cannonBallPool);
    }

    protected override void Target()
    {
        IEnumerable<Entity> targets = TowerManager.instance.Towers
            .Where(t =>
                t.IsAlive &&
                !t.Hiding &&
                rangeCollider.OverlapPoint(t.transform.position) &&
                IsDetectable(t.transform.position))
            .Select(t => t as Entity);

        if (targets.Any())
        {
            this.CurrentTarget = new EntityTarget(targets.First());
        }
    }

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

    protected override void WalkToEntity(Entity entity, float radius)
    {
        walker.WalkOnPathRanged(entity.transform.position, radius, obstacleLayer, () =>
        {
            // stop motivation check and start attacking when arrived at target
            if (motivationCycle != null) StopCoroutine(motivationCycle);
            attackCycle = StartCoroutine(Attack());
        });

        // see if still motivated to get to entity
        motivationCycle = StartCoroutine(MotivationCheck());
    }
}
