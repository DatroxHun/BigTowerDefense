using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

    public static IEnumerable<Func<TowerComponent>> GetAll()
    {
        // Search for Static Properties
        var properties = typeof(ComponentLibrary).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(TowerComponent));

        // Return each property
        foreach (var prop in properties)
        {
            yield return () =>
            {
                return (TowerComponent)prop.GetValue(null);
            };
        }
    }

    // COMPONENTS
    // DirectDamage = DDMG
    // ElectricDamage = EDMG
    // FireDamage = FDMG
    // PhysicalDamage = PHDMG
    // PoisonDamage = PODMG

    public static TowerComponent RangeUpgrade => new TowerComponent
    (
        new StatAlteration(new List<(TowerStats, float, float)> { (TowerStats.TowerRange, 0, 1.2f) }),
        null, null,
        Sprites["range"], new (int, int)[]
        {
            (0, 0), (1, 0),
                    (1, 1)
        },
        "Radar Module", "+20% range", 100        
    );

    public static TowerComponent ElectricDamageUpgrade => new TowerComponent
    (
        new StatAlteration(new List<(TowerStats, float, float)> { (TowerStats.ElectricDamage, 2, 1.1f) }),
        null, null,
        Sprites["range"], new (int, int)[]
        {
            (0, 0), (1, 0),
                    (1, 1)
        },
        "Battery overload", "+2 electric damage and +10% electric damage", 100
    );

    public static TowerComponent MachineGunUpgrade => new TowerComponent(
            new StatAlteration(new List<(TowerStats, float, float)> { (TowerStats.PhysicalDamage, 0, 0.5f), (TowerStats.PhysicalDamage, 0, 0.5f), (TowerStats.ElectricDamage, 0, 0.5f), (TowerStats.FireDamage, 0, 0.5f), (TowerStats.DirectDamage, 0, 0.5f), (TowerStats.PoisonDamage, 0 ,0.5f) }),
            null,
            new AdvancedTargettingAlteration((y) => (y.Item1 , y.Item2 + 1)),
            Sprites["poison"], new (int, int)[]
            {
                (0, 0), (1, 0), (2, 0),
                        (1, 1),
                        (1, 2)
            },
        "Machinegun", "Tower now fires twice at enemies, all damage halved", 350
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
        },
        "Poison Dart Frog Capsule", "5 PODMG / 0.4s", 350
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
