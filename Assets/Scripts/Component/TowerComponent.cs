using System.Linq;
using UnityEngine;
#nullable enable
public class TowerComponent
{
    public StatAlteration? StatAlteration { get; private set; } // they are null if there is none
    public AdvancedAttackAlteration? AdvancedAttackAlteration { get; private set; }
    public AdvancedTargettingAlteration? AdvancedTargettingAlteration { get; private set; }

    // UI
    public Vector2Int position { get; set; }
    public Sprite? Image { get; private set; }
    public Vector2Int[]? Shape { get; private set; }
    public Vector2Int? Size { get; private set; }

    public TowerComponent(StatAlteration? statAlteration, 
                          AdvancedAttackAlteration? advancedAttackAlteration, 
                          AdvancedTargettingAlteration? advancedTargettingAlteration, 
                          Sprite? image, Vector2Int[]? shape)
    {
        StatAlteration = statAlteration;
        AdvancedAttackAlteration = advancedAttackAlteration;
        AdvancedTargettingAlteration = advancedTargettingAlteration;

        Image = image;
        Shape = shape;
        
        if (Shape != null && Shape.Length > 0)
            Size = new Vector2Int(Shape.Max(c => c.x) + 1, Shape.Max(c => c.y) + 1);
    }

    public TowerComponent(StatAlteration? statAlteration,
                          AdvancedAttackAlteration? advancedAttackAlteration,
                          AdvancedTargettingAlteration? advancedTargettingAlteration,
                          Sprite? image, (int, int)[]? shape) : 
        this(statAlteration, advancedAttackAlteration, 
             advancedTargettingAlteration, image, shape.Select(x => new Vector2Int(x.Item1, x.Item2)).ToArray())
    { }
}
