using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[DefaultExecutionOrder(-100)]
public class ComponentLibrary : MonoBehaviour
{
    private static ComponentLibrary instance;

    // Reference for Sprites
    [SerializeField] private SpriteHolder sprites;
    private static SpriteHolder Sprites { get => instance.sprites; }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(this);
    }

    // Components

    public static TowerComponent RangeUpgrade => new TowerComponent
    (
        new StatAlteration(new List<(TowerStats, float, float)> { (TowerStats.TowerRange, 0, 1.2f) }),
        null, null,
        Sprites["range"], new (int, int)[]
        {
            (0, 0), (1, 0),
                    (1, 1)
        }
    );

    public static TowerComponent PoisonComponent => new TowerComponent
    (
        new(new() { (TowerStats.PoisonDamage, 5, 1) }),
        new AdvancedAttackAlteration(new List<TowerStats>() { TowerStats.PoisonDamage }, PoisonLogic),
        null,
        Sprites["poison"], new (int, int)[]
        {
            (0, 0), (1, 0), (2, 0),
                    (1, 1),
                    (1, 2)
        }
    );

    private static IEnumerator PoisonLogic(Dictionary<TowerStats,float> stats,Enemy enemy)
    {
        var wait = new WaitForSeconds(0.4f);
        while (enemy.IsAlive)
        {
            Debug.Log($"Poisoned for: {stats[TowerStats.PoisonDamage]}");
            enemy.ApplyDamage(new DamageObj() { poison = stats[TowerStats.PoisonDamage] });
            yield return wait;
        }
    }
}
