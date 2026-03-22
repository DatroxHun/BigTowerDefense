using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour, IPoolable
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
    private Action<Enemy> impactEffect;

    private bool isFlying = false;

    public GameObject Object => throw new NotImplementedException();

    public IObjectPool<IPoolable> Pool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Launch(Vector3 target, Action<Enemy> impactEffect)
    {
        this.target = target;
        this.impactEffect = impactEffect;
    }

    public void SpawnAction(Vector3 position)
    {
        this.transform.position = position;
        isFlying = true;
    }

    public void Return2Pool()
    {
        isFlying = false;
        Pool.Release(this);
    }

    void Update()
    {
        if (!isFlying)
            return;

        this.transform.position =
            Vector3.MoveTowards(this.transform.position, target, speed * Time.deltaTime);


        if (Vector3.Distance(transform.position, target) < 0.1f || CheckObstacle())
            Arrive();

    }
    private bool CheckObstacle() => 
        Physics2D.OverlapPoint(this.transform.position, obstacleLayer) ||
        Physics2D.OverlapPoint(this.transform.position, enemyLayer);

    void Arrive()
    {
        isFlying = false;
        
        List<Enemy> enemies = EnemyManager.instance.Enemies;

        enemies
            .Where(t =>
                impactCollider.OverlapPoint(t.transform.position))
            .ToList()
            .ForEach(enemy => impactEffect?.Invoke(enemy));

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("GameBounds"))
        {
            isFlying = false;
        }
    }

}
