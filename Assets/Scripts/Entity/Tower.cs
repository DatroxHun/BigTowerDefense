#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public struct TowerStatsFloatTuple
{
    public TowerStats towerStats;
    public float value;

    public TowerStatsFloatTuple(TowerStats towerStats, float value)
    {
        this.towerStats = towerStats;
        this.value = value;
    }
}

public abstract class Tower : Entity
{
    [field: SerializeField]
    public int Price { get; private set; }

    [SerializeField] protected Vector2Int moduleSize = new Vector2Int(11, 5);

    protected ComponentModule module = null!;
    protected bool idle = true;
    private Coroutine acting = null!;
    [SerializeField] private List<TowerStatsFloatTuple> baseStatsInit = new List<TowerStatsFloatTuple>();
    private Dictionary<TowerStats, float> BaseStats;
    public Dictionary<TowerStats, float> CurrentStats { get { return module.UpdateStats(BaseStats); } }
    public List<ComponentType> allowedTypes = new List<ComponentType>();

    protected override bool Invulnerable() => Hiding;

    [SerializeField] protected SpriteRenderer sprite;
    [SerializeField] protected ParticleSystem? fireParticleSystem = null;


    [SerializeField] protected float actiondelaySeconds = 2.0f;
    public override float MaxHitPoints { get => CurrentStats[TowerStats.TowerMaxHitPoints]; }
    protected override DamageObj AttackDamage { get => new DamageObj{ direct = CurrentStats.GetValueOrDefault(TowerStats.DirectDamage,0), electric = CurrentStats.GetValueOrDefault(TowerStats.ElectricDamage, 0), fire = CurrentStats.GetValueOrDefault(TowerStats.FireDamage, 0), physical = CurrentStats.GetValueOrDefault(TowerStats.PhysicalDamage, 0), poison = CurrentStats.GetValueOrDefault(TowerStats.PoisonDamage, 0) }; }
    protected override DamageObj Vulnerabilities { get => new DamageObj { direct = CurrentStats.GetValueOrDefault(TowerStats.DirectDamageVulnerability, 1), electric = CurrentStats.GetValueOrDefault(TowerStats.ElectricDamageVulnerability, 1), fire = CurrentStats.GetValueOrDefault(TowerStats.FireDamageVulnerability, 1), physical = CurrentStats.GetValueOrDefault(TowerStats.PhysicalDamageVulnerability, 1), poison = CurrentStats.GetValueOrDefault(TowerStats.PoisonDamageVulnerability, 1) }; }

    protected float Range
    {
        get { return rangeCollider.radius; }
        set { rangeCollider.radius = value; }
    }

    public bool Hiding { get; protected set; } = false;

    private void Awake()
    {
        module = new ComponentModule(moduleSize);

        BaseStats = baseStatsInit.ToDictionary(stat => stat.towerStats, stat => stat.value);
        //Debug.Log(BaseStats.Count);
        HitPoints = MaxHitPoints;
    }

    protected new void Start()
    {
        
        base.Start();

        transform.parent = TowerManager.instance.transform;
        TowerManager.instance.AddTower(this);
        
        acting = StartCoroutine(Acting());
    }

    private void Update()
    {
        
    }

    // hide as soon as done doing thing
    public void ToggleHide(System.Action callback = null!)
    {
        if (!IsAlive)
            return;

        if (!Hiding)
        {
            StartCoroutine(HideASAP(callback));
        }
        else
        {
            Hiding = false;
            sprite.color = Color.white;

            callback?.Invoke();
        }
        //handle visual stuff
    }

    protected IEnumerator HideASAP(System.Action callback = null!)
    {
        yield return new WaitUntil(() => idle);
        Hiding = true;
        sprite.color = Color.black;

        callback?.Invoke();
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
        // stop fire
        if (fireParticleSystem != null)
            fireParticleSystem.Stop();

        // repair
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

        if (fireParticleSystem != null)
            fireParticleSystem.Play();

        base.JustDied();
    }

    public void LoadInventory()
    {
        //Debug.Log($"Allowed Type: {allowedTypes[0]}");
        InventoryManager.RefreshBar(allowedTypes);
        InventoryManager.ResetComponentModule(module);
    }

    public void Sell()
    {
        BuildingManager.instance.SellTower(this);
    }
}
