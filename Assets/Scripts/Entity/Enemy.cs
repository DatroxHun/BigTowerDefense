#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : Entity, IPoolable
{
    // Attack
    public float DetectionRange
    {
        get => rangeCollider.radius;
        private set => rangeCollider.radius = value;
    }

    // Walking
    [SerializeField] private Walker walker = null!;
    private Road road = null!;
    public void SetRoad(Road road) => this.road = road;


    // IPoolable
    public GameObject Object => gameObject;
    public IObjectPool<IPoolable> Pool { get; set; } = null!;
    public void Return2Pool()
    {
        // stop cycles        
        StopAllCycles();

        Pool.Release(this);
    }

    public void SpawnAction(Vector3 position)
    {
        ParticlePool.instance.Emit(position);

        transform.position = position;
        CurrentTarget = null;

        // regenerate health
        HitPoints = MaxHitPoints;

        WalkOnRoad();
        //walker.WalkOnRoad(road, globalCallback: Return2Pool);
    }

    // Brain Cycles
    const float scanInterval = 0.5f;
    const float motivationCheckInterval = 0.5f;
    float attackInterval = 1f;

    Coroutine? scanCycle = null;
    Coroutine? motivationCycle = null;
    Coroutine? attackCycle = null;

    private void StopAllCycles()
    {
        if (scanCycle != null) StopCoroutine(scanCycle);
        if (motivationCycle != null) StopCoroutine(motivationCycle);
        if (attackCycle != null) StopCoroutine(attackCycle);
    }

    private void WalkOnRoad(float scanDelay = 0f)
    {
        walker.WalkOnRoad(road, globalCallback: () =>
        {
            // at the end of the road find base tower
            StopAllCycles();
            scanCycle = StartCoroutine(Scan());
        });

        // start scanning on road
        if (scanCycle != null) StopCoroutine(scanCycle);
        scanCycle = StartCoroutine(Scan(scanDelay));
    }
 
    private void WalkToEntity(Entity entity, float radius)
    {
        walker.WalkOnPath(entity.transform.position, radius, () => 
        {
            // stop motivation check and start attacking when arrived at target
            if (motivationCycle != null) StopCoroutine(motivationCycle);
            attackCycle = StartCoroutine(Attack());
        });

        // see if still motivated to get to entity
        motivationCycle = StartCoroutine(MotivationCheck());
    }

    private IEnumerator Scan(float scanSilenceInterval = 0f)
    {
        yield return new WaitForSeconds(scanSilenceInterval);

        bool doScan = true;
        while (doScan)
        {
            yield return new WaitForSeconds(scanInterval);

            // keep searching for target until finding one         
            Target();

            if (CurrentTarget is not null) // found target
            {
                doScan = false;

                if (CurrentTarget is EntityTarget target)
                {
                    WalkToEntity(target.entity, 1f); // hard coded radius!!!!!!!!
                }
                else
                {
                    throw new System.Exception("Not intended target class!");
                }
            }
        }
    }

    private IEnumerator MotivationCheck()
    {
        Vector3? prevPos = null;

        bool doCheck = true;
        while (doCheck)
        {
            yield return new WaitForSeconds(motivationCheckInterval);

            // get target entity
            Entity? tower = null;
            if (CurrentTarget is EntityTarget target && target.entity != null)
            {
                tower = target.entity;
            }
            else
            {
                throw new System.Exception("Not intended target class!");
            }

            // calculate average speed for last motivationCheckInterval
            float avgSpeed = float.PositiveInfinity;
            if (prevPos != null)
                avgSpeed = Vector3.Distance(walker.transform.position, prevPos.Value) / motivationCheckInterval;

            // if not pathfinding or avg speed is too low or entity no longer exists -> go back on road and scan
            if (walker.Mode != WalkModes.Pathfind || avgSpeed < walker.Speed / 4f || tower == null || !tower.IsAlive)
            {
                doCheck = false;
                CurrentTarget = null;
                WalkOnRoad(scanDelay: 5f); // starts scanning after delay
            }

            prevPos = walker.transform.position;
        }
    }

    private IEnumerator Attack()
    {
        bool doAttack = true;
        while (doAttack)
        {
            yield return new WaitForSeconds(attackInterval);

            Debug.Log("a");

            if (CurrentTarget is EntityTarget target)
            {
                // if target is alive -> attack
                if (target.entity != null && target.entity.IsAlive)
                {
                    Action();
                }
                else // if not alive -> stop attacking and got back to road
                {
                    doAttack = false;
                    CurrentTarget = null;
                    WalkOnRoad(scanDelay: 1f); // starts scanning after delay 
                }
            }
            else
            {
                throw new System.Exception("Not intended target class!");
            }
        }
    }

    // Actions
    protected override void Action()
    {
        Debug.Log("Enemy Action");

        if (CurrentTarget is EntityTarget target && target.entity != null)
        {
            DamageObj dmg = new DamageObj()
            {
                physical = 5f
            };

            target.entity.ApplyEffect(Effects.InstantDamage(target.entity, dmg));
        }
    }

    protected override void Target()
    {
        IEnumerable<Entity> targets = TowerManager.instance.Towers
            .Where(t => t.IsAlive && rangeCollider.OverlapPoint(t.transform.position))
            .Select(t => t as Entity);

        if (targets.Any())
        {
            this.CurrentTarget = new EntityTarget(targets.First());
        }
    }

    protected override void JustDied()
    {
        base.JustDied();
        Return2Pool();
    }
}
