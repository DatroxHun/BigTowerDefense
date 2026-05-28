#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public abstract class Enemy : Entity, IPoolable
{
    [SerializeField]
    protected Animator animator;

    // Attack
    public float DetectionRange
    {
        get => rangeCollider.radius;
        private set => rangeCollider.radius = value;
    }

    // Walking
    [SerializeField] protected Walker walker = null!;
    private Road road = null!;
    public void SetRoad(Road road) => this.road = road;

    [SerializeField]
    protected float attackRange = 1f;

    // IPoolable
    public GameObject Object => gameObject;
    public IObjectPool<IPoolable> Pool { get; set; } = null!;

    // Productivity
    protected HashSet<Tower> blackList = new();

    // Brain Cycles
    const float scanInterval = 0.5f;
    const float motivationCheckInterval = 0.5f;
    [SerializeField] protected float attackInterval = 1f;

    protected Coroutine? scanCycle = null;
    protected Coroutine? motivationCycle = null;
    protected Coroutine? attackCycle = null;


    #region Life Course

    private void TurnSprite(float currentX)
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        Vector3 spriteScale = sprite.transform.localScale;
        float turn = spriteScale.x * currentX < 0 ? -1f : 1f; // need to turn?
        sprite.transform.localScale = new Vector3(turn * spriteScale.x, spriteScale.y, spriteScale.z);
    }

    void OnEnable()
    {
        walker?.OrientationChanged.RemoveAllListeners();
        walker?.OrientationChanged.AddListener(TurnSprite);
    }

    public void SpawnAction(Vector3 position)
    {
        // emit smoke
        ParticlePool.Emit(position, ParticleType.Smoke);

        // set position and reset target
        transform.position = position;
        CurrentTarget = null;

        // regenerate health
        HitPoints = MaxHitPoints;

        WalkOnRoad();

        // reset productivity check    
        blackList.Clear();
    }

    public void Return2Pool()
    {
        // reset animation
        ResetVisuals();

        // stop cycles and reset productivity check    
        StopAllCycles();
        blackList.Clear();

        Pool.Release(this);
    }

    protected override void JustDied()
    {
        base.JustDied();
        Return2Pool();
    }

    #endregion

    #region Brain

    private void StopAllCycles()
    {
        if (scanCycle != null) StopCoroutine(scanCycle);
        if (motivationCycle != null) StopCoroutine(motivationCycle);
        if (attackCycle != null) StopCoroutine(attackCycle);
    }

    /// <summary>
    /// Start walking on asigned road. After specified delay starts scanning.
    /// </summary>
    /// <param name="scanDelay">scanning start delay</param>
    protected void WalkOnRoad(float scanDelay = 0f)
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
 
    /// <summary>
    /// Start walking towards specified enemy. Stop when desired distance is reached.
    /// </summary>
    /// <param name="entity">entity to walk to</param>
    /// <param name="radius">distance to stop at</param>
    protected virtual void WalkToEntity(Entity entity, float radius)
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

            // found target
            if (CurrentTarget is not null)
            {
                doScan = false;

                if (CurrentTarget is EntityTarget target)
                {
                    WalkToEntity(target.entity, attackRange);
                }
                else
                {
                    Debug.LogError("Not intended target class!");
                }
            }
        }
    }

    protected IEnumerator MotivationCheck()
    {
        bool doCheck = true;
        Vector3? prevPos = null;

        void LoseInterest()
        {
            doCheck = false;
            CurrentTarget = null;
            WalkOnRoad(scanDelay: 5f); // starts scanning after delay
        }

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
                // if target for any reason not entity
                LoseInterest();
            }

            // calculate average speed for last motivationCheckInterval
            float avgSpeed = float.PositiveInfinity;
            if (prevPos != null)
                avgSpeed = Vector3.Distance(walker.transform.position, prevPos.Value) / motivationCheckInterval;

            // if not pathfinding OR avg speed is too low OR entity no longer exists
            if (walker.Mode != WalkModes.Pathfind || avgSpeed < walker.Speed / 4f || tower == null || !tower.IsAlive)
            {
                // go back on road and scan
                LoseInterest();
            }

            // update prevPos
            prevPos = walker.transform.position;
        }
    }

    protected virtual IEnumerator Attack()
    {
        bool doAttack = true;
        
        void StopAttack()
        {
            doAttack = false;
            CurrentTarget = null;
            WalkOnRoad(scanDelay: 1f); // starts scanning after delay
        }

        // ensure a valid target before starting
        if (CurrentTarget is not EntityTarget initialTarget || initialTarget.entity == null)
            yield break;

        // productivity
        int attackCounter = 0;
        float targetHealthSnapshot = initialTarget.entity.HitPoints;

        // rotate sprite correctly
        TurnSprite(initialTarget.entity.transform.position.x - transform.position.x);

        while (doAttack)
        {
            yield return new WaitForSeconds(attackInterval);

            if (CurrentTarget is EntityTarget target)
            {
                // check if the tower was destroyed or hid WHILE waiting
                if (target.entity == null || !target.entity.IsAlive || (target.entity is Tower t_hide && t_hide.Hiding))
                {
                    StopAttack();
                    yield break;
                }

                // Productivity Check
                if (attackCounter > 0 && attackCounter % 5 == 0)
                {
                    // if health stayed the same or increased (except BaseTower)
                    if (target.entity.HitPoints >= targetHealthSnapshot && target.entity is Tower t && t is not BaseTower)
                    {
                        // add to blackList and move on
                        blackList.Add(t);
                        StopAttack();
                        yield break;
                    }

                    // update snapshot
                    targetHealthSnapshot = target.entity.HitPoints;
                }

                // target is confirmed alive and we are productive, attack!
                Action();
                attackCounter++;
            }
            else
            {
                Debug.LogError("Not intended target class!");
            }
        }
    }

    #endregion

    #region Attack

    // protected override void Target()
    protected virtual void Target()
    {
        IEnumerable<Entity> targets = TowerManager.instance.Towers
            .Where(t =>
                t != null &&                                        // not null
                t.IsAlive &&                                        // alive
                !t.Hiding &&                                        // not hiding
                !blackList.Contains(t) &&                           // not blacklisted
                rangeCollider.OverlapPoint(t.transform.position))   // inside range
            .Select(t => t as Entity);

        if (targets.Any())
        {
            this.CurrentTarget = new EntityTarget(targets.First());
        }
    }

    public override void ApplyDamage(DamageObj dobj)
    {
        // trigger hurt animation
        animator.SetTrigger("hurt");

        // apply damage the usual way
        base.ApplyDamage(dobj);
    }

    #endregion

    /// <summary>
    /// Reset animated properties.
    /// </summary>
    protected void ResetVisuals()
    {
        // reset sprite
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        sr.color = Color.white;
        sr.transform.localScale.Set(
            Mathf.Abs(sr.transform.localScale.x), 
            sr.transform.localScale.y, 
            sr.transform.localScale.z
        );

        // reset animation
        Animator anim = GetComponentInChildren<Animator>();

        anim.gameObject.transform.position = Vector3.zero;
        anim.gameObject.transform.localRotation = Quaternion.identity;
        anim.gameObject.transform.localScale = Vector3.one;

        anim.Rebind();
        anim.Update(0);
        anim.Play("slime");
    }
}