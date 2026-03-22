using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LongRangeTower : Tower
{
    [SerializeField]
    private LayerMask obstacleLayer;


    protected override void Action()
    {
        throw new System.NotImplementedException();
    }

    protected override void Target()
    {
        List<Enemy> enemies = EnemyManager.instance.Enemies;

        

        List<ITarget> targets = enemies
            .Where(t =>
                rangeCollider.OverlapPoint(t.transform.position) && 
                IsDetectable(t.transform.position))
                .Select(e => e as ITarget).ToList();

        this.CurrentTarget = new MultiTarget(targets);

        // for testing; delete later
        // - K
        Debug.Log($"[GUNNER] : ELIGABLE TARGETS: {targets.Count}");
        /*
        foreach (var target in targets)
        {
            Enemy e = target as Enemy;
            Debug.Log(e.transform.position);
        }
         */
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
