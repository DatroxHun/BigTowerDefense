using UnityEngine;
using System.Collections;

public abstract class TargetingTower : Tower
{
    protected abstract void Target();

    private Coroutine targeting = null!;
    
    /// <summary>
    /// Time between (re)targeting attempts
    /// </summary>
    [SerializeField]
    protected float retargetDelaySeconds = 10.0f;

    /// <summary>
    /// Time it takes to choose a target
    /// </summary>
    [SerializeField]
    protected float targetTimeSeconds = 2.0f;

    protected new void Start()
    {
        base.Start();
        targeting = StartCoroutine(Targeting());
    }

    private IEnumerator Targeting()
    {
        while (true)
        {
            if (idle)
            {
                idle = false;
                yield return new WaitForSeconds(targetTimeSeconds);
                //Debug.Log($"[TOWER] : TARGETIMG");
                Target();
                idle = true;
            }

            yield return new WaitForSeconds(retargetDelaySeconds);
            yield return new WaitUntil(() => !Hiding);
        }
    }

    protected override void OnDestruction()
    {
        base.OnDestruction();

        SafeStopCoroutine(targeting);
    }

    public override void OnRepair()
    {
        base.OnRepair();

        SafeStopCoroutine(targeting);
        targeting = StartCoroutine(Targeting());
    }
}
