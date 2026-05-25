using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed = 1.0f;

    [SerializeField]
    private LayerMask enemyLayer;
    [SerializeField]
    private LayerMask obstacleLayer;

    [SerializeField]
    private CircleCollider2D impactCollider;

    private Vector3 target;
    private Action<Entity> impactEffect;

    private bool isFlying = false;

    public GameObject Object => gameObject;

    private ObjectPool<Bullet> pool;
    public void Launch(Vector3 target, Action<Entity> impactEffect, ObjectPool<Bullet> pool)
    {
        this.pool = pool;
        this.target = target;
        this.impactEffect = impactEffect;
        isFlying = true;
        ParticlePool.Emit(transform.position, ParticleType.Smoke, sizeMultiplier: 0.3f);
    }

    void Update()
    {
        if (!isFlying)
            return;

        this.transform.position =
            Vector3.MoveTowards(this.transform.position, target, speed * Time.deltaTime);
        
        // idea: allow bullet to go further, not just "hit the ground" at the target

        if (Vector3.Distance(transform.position, target) < 0.001f)
        {
            Debug.Log($"[BULLET] : HIT TARGET");
            Arrive();
        }
        else if (CheckObstacle())
        {
            Debug.Log($"[BULLET] : HIT OBSTACLE");
            Arrive();
        }

    }

    // commented section is for enemies to stop the bullets
    private bool CheckObstacle() =>
        Physics2D.OverlapPoint(this.transform.position, obstacleLayer);// ||
        //Physics2D.OverlapPoint(this.transform.position, enemyLayer);

    void Arrive()
    {
        isFlying = false;
        
        List<Entity> enemies = EnemyManager.instance.Enemies.Select(e => e as Entity).ToList();
        List<Entity> towers = TowerManager.instance.Towers.Select(t => t as Entity).ToList();
        List<Entity> entities = enemies.Concat(towers).ToList();

        // if enemies have a collider, then here we need to
        // check if the impactC and the enemyC overlap !
        // -K
        
        // if not overlap is written here and enemies stop bullets -> breaks damaging!
        entities
            .Where(e =>
                impactCollider.OverlapPoint(e.transform.position))
            .ToList()
            .ForEach(hitEntity => impactEffect?.Invoke(hitEntity));

        pool.Release(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("GameBounds"))
        {
            isFlying = false;
            Debug.Log($"[BULLET] : ESCAPED");
            pool.Release(this);
        }
    }

}
