using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed = 10.0f;
    [SerializeField]
    private LayerMask enemyLayer;
    [SerializeField]
    private LayerMask obstacleLayer;
    [SerializeField]
    private CircleCollider2D impactCollider;

    private Vector3 target;
    private Action<Enemy> impactEffect;

    private bool isFlying = false;

    public void Launch(Vector3 target, Action<Enemy> impactEffect)
    {
        this.target = target;
        this.impactEffect = impactEffect;
        isFlying = true;
    }

    void Start()
    {

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
}
