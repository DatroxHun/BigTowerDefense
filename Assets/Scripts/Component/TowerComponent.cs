using NUnit.Framework;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
#nullable enable
public class TowerComponent
{
    // Functional
    public StatAlteration? StatAlteration { get; private set; } // they are null if there is none
    public AdvancedAttackAlteration? AdvancedAttackAlteration { get; private set; }
    public AdvancedTargettingAlteration? AdvancedTargettingAlteration { get; private set; }

    // UI
    public Vector2Int position { get; set; }
    public Sprite? Image { get; private set; }
    public Vector2Int[]? Shape { get; set; }
    public Vector2Int Size { get => Shape != null && Shape.Length > 0 ? new Vector2Int(Shape.Max(c => c.x) + 1, Shape.Max(c => c.y) + 1) : new Vector2Int(0,0); }

    // Economy
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Price { get; private set; }

    public List<ComponentType> Types = new();

    public TowerComponent(StatAlteration? statAlteration, 
                          AdvancedAttackAlteration? advancedAttackAlteration, 
                          AdvancedTargettingAlteration? advancedTargettingAlteration, 
                          Sprite? image, Vector2Int[]? shape,
                          string? name, string? desc, int? price, List<ComponentType> types)
    {
        StatAlteration = statAlteration;
        AdvancedAttackAlteration = advancedAttackAlteration;
        AdvancedTargettingAlteration = advancedTargettingAlteration;

        Image = image;
        Shape = shape;

        Name = name ?? "Unknown Component";
        Description = desc ?? string.Empty;
        Price = price ?? 0;
        Types = types;
    }

    public TowerComponent(StatAlteration? statAlteration,
                          AdvancedAttackAlteration? advancedAttackAlteration,
                          AdvancedTargettingAlteration? advancedTargettingAlteration,
                          Sprite? image, (int, int)[]? shape,
                          string? name, string? desc, int? price, List<ComponentType> types) : 
        this(statAlteration, advancedAttackAlteration, advancedTargettingAlteration, 
             image, shape.Select(x => new Vector2Int(x.Item1, x.Item2)).ToArray(), 
             name, desc, price, types)
    { }
}
