using NUnit.Framework;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Splines.ExtrusionShapes;
using static UnityEngine.GraphicsBuffer;


public class ComponentModule { }

public abstract class Tower : Entity
{
    protected ComponentModule module;
    protected bool hiding = false;
    protected bool idle = true;
    private Coroutine targeting = null!;
    private Coroutine acting = null!;

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


    [SerializeField]
    protected float actiondelaySeconds = 2.0f;

    protected float Range
    {
        get { return rangeCollider.radius; }
        set { rangeCollider.radius = value; }
    }

    private void Awake()
    {
        HitPoints = MaxHitPoints;
    }

    protected void Start()
    {
        transform.parent = TowerManager.instance.transform;
        TowerManager.instance.AddTower(this);

        targeting = StartCoroutine(Targeting());
        acting = StartCoroutine(Acting());
    }

    private void Update()
    {
        
    }

    // hide as soon as done doing thing
    protected void ToggleHide()
    {
        if (!hiding)
            StartCoroutine(HideASAP());
        else
            hiding = false;
        //handle visual stuff
    }

    protected IEnumerator HideASAP()
    {
        yield return new WaitUntil(() => idle);
        hiding = true;
    }

    private void SafeStopCoroutine(Coroutine cr)
    {
        if (cr != null)
            StopCoroutine(cr);
    }

    private void OnDestruction()
    {
        SafeStopCoroutine(targeting);
        SafeStopCoroutine(acting);
    }


    // call when wave ends / before wave begins
    protected void OnRepair()
    {
        // avoid duplicate coroutines
        SafeStopCoroutine(targeting);
        SafeStopCoroutine(acting);

        // maybe TODO: StopCoroutines function

        // start for real
        targeting = StartCoroutine(Targeting());
        acting = StartCoroutine(Acting());
    }

    private IEnumerator Targeting()
    {
        while(true)
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
            yield return new WaitUntil(() => !hiding);
        }
    }

    private IEnumerator Acting()
    {
        // idea: do idle status checking with WaitUntil instead of this
        // because it could cause some weird patterns in time
        // -K
        while (true)
        {
            if (idle)
            {
                idle = false;

                // még nincs ilyen
                //yield return new WaitForSeconds(actionTimeSeconds);
                //Debug.Log($"[TOWER] : ACTING");
                Action();
                idle = true;
            }

            yield return new WaitForSeconds(actiondelaySeconds);
            yield return new WaitUntil(() => !hiding);
        }
    }

    protected override void JustDied()
    {
        OnDestruction();
        base.JustDied();   
    }
}
