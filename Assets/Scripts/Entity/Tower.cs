using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;
using UnityEngine.Pool;
using System.Collections;
using NUnit.Framework;
using Unity.VisualScripting;

public class ComponentModule { }

public abstract class Tower : Entity
{
    protected ComponentModule module;
    protected bool hiding = false;
    protected bool idle = true;
    private Coroutine targeting = null!;


    // protected Behavior b;

    private void Start()
    {
        targeting = StartCoroutine(Targeting());


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

    protected void OnDestruction()
    {
        StopCoroutine(targeting);
    }

    protected void OnRepair()
    {
        targeting = StartCoroutine(Targeting());
    }

    private IEnumerator Targeting()
    {
        while(true)
        {
            if (idle)
            {
                idle = false;
                yield return new WaitForSeconds(targetTimeSeconds);
                Target();
                idle = true;
            } 

            yield return new WaitForSeconds(retargetDelaySeconds);
            yield return new WaitUntil(() => !hiding);

        }

    }
}
