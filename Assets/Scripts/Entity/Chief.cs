using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class Chief : Cannoneer
{
    [SerializeField]
    private int clusterShotSize = 1;

    protected override void WalkToEntity(Entity entity, float radius)
    {
        walker.Stop(() =>
        {
            // stop motivation check and attack immediately
            if (motivationCycle != null) StopCoroutine(motivationCycle);
            attackCycle = StartCoroutine(Attack());
        });
        
        // check motivation later
        motivationCycle = StartCoroutine(MotivationCheck());
    }

    protected override IEnumerator Attack()
    {

        //bool doAttack = true;
        //while (doAttack)
        //{
            if (CurrentTarget is EntityTarget target)
            {
                // if target is alive -> attack
                if (target.entity != null && target.entity.IsAlive && target.entity is Tower tower && !tower.Hiding)
                {
                    for (int i = 0; i < clusterShotSize; i++)
                    {
                        yield return new WaitForSeconds(attackInterval);
                        Action();
                    }
                }
            /*
            else // if not alive -> stop attacking and got back to road
            {
                doAttack = false;
                CurrentTarget = null;
                WalkOnRoad(scanDelay: 1f); // starts scanning after delay 
            }
            */

            CurrentTarget = null;
            WalkOnRoad(scanDelay: 5f);
            }
            else
            {
                throw new System.Exception("Not intended target class!");
            }
        //}
    }

}
