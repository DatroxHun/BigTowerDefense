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
    protected bool idle = true;
    private Coroutine acting = null!;

    protected override bool Invulnerable() => Hiding;

    [SerializeField]
    protected SpriteRenderer sprite;

    [SerializeField]
    protected float actiondelaySeconds = 2.0f;

    protected float Range
    {
        get { return rangeCollider.radius; }
        set { rangeCollider.radius = value; }
    }

    public bool Hiding { get; protected set; } = false;

    private void Awake()
    {
        HitPoints = MaxHitPoints;
    }

    protected void Start()
    {
        transform.parent = TowerManager.instance.transform;
        TowerManager.instance.AddTower(this);

        acting = StartCoroutine(Acting());
    }

    private void Update()
    {
        
    }

    // hide as soon as done doing thing
    public void ToggleHide()
    {
        if (!IsAlive)
            return;

        if (!Hiding)
        {
            StartCoroutine(HideASAP());
            sprite.color = Color.black;
        }
        else
        {
            Hiding = false;
            sprite.color = Color.white;
        }
        //handle visual stuff
    }

    protected IEnumerator HideASAP()
    {
        yield return new WaitUntil(() => idle);
        Hiding = true;
    }

    protected void SafeStopCoroutine(Coroutine cr)
    {
        if (cr != null)
            StopCoroutine(cr);
    }

    protected virtual void OnDestruction()
    {
        SafeStopCoroutine(acting);
    }


    // call when wave ends / before wave begins
    public virtual void OnRepair()
    {
        HitPoints = MaxHitPoints;

        // avoid duplicate coroutines
        
        SafeStopCoroutine(acting);

        // start for real
        
        acting = StartCoroutine(Acting());

        idle = true;
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
            yield return new WaitUntil(() => !Hiding);
        }
    }

    protected override void JustDied()
    {
        OnDestruction();
        base.JustDied();   
    }
}
