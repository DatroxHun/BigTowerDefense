using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed = 4.0f;

    [SerializeField]
    private LayerMask enemyLayer;
    [SerializeField]
    private LayerMask obstacleLayer;

    [SerializeField]
    private CircleCollider2D impactCollider;

    private Action<Entity> impactEffect;

    [SerializeField]
    private Rigidbody2D bulletBody;

    public GameObject Object => gameObject;

    private ObjectPool<Bullet> pool;
    public void Launch(Vector3 target, Action<Entity> impactEffect, ObjectPool<Bullet> pool)
    {
        this.pool = pool;
        this.impactEffect = impactEffect;
        ParticlePool.Emit(transform.position, ParticleType.Smoke, sizeMultiplier: 0.3f);
        Vector3 direction = (target - transform.position).normalized;
        bulletBody.linearVelocity = direction * speed;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;

        if ((enemyLayer & (1 << otherLayer)) != 0)
        {

            Entity hitEntity = other.GetComponent<Entity>();
            Debug.Log($"[BULLET] : HIT ENEMY");
            if (hitEntity != null)
            {
                impactEffect.Invoke(hitEntity);
            }

            Cleanup();
        }
        else if ((obstacleLayer & (1 << otherLayer)) != 0)
        {
            Debug.Log($"[BULLET] : HIT OBSTACLE");
            Cleanup();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("GameBounds"))
        {
            Debug.Log($"[BULLET] : ESCAPED");
            //pool.Release(this);
        }
    }

    private void Cleanup()
    {
        bulletBody.linearVelocity = Vector2.zero;
        bulletBody.angularVelocity = 0f;
        pool.Release(this);
    }

}
