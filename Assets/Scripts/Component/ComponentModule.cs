using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class ComponentModule
{
    public List<TowerComponent> Components { get; private set; } = new List<TowerComponent>();
    private bool _isAmpDirty = true;
    private Dictionary<TowerStats, float> cache = new();
    private Dictionary<TowerStats,(float,float)> _amplificationProvider = new Dictionary<TowerStats, (float, float)>();

    public Vector2Int Size { get; private set; }
    private TowerComponent[,] grid;

    public ComponentModule(Vector2Int size)
    {
        this.Size = size;
        grid = new TowerComponent[size.x, size.y];
    }

    /*
    public string GenerateDescription(String towerDescription, Dictionary<TowerStats, float> stats) // style later, or maybe instead of putting the string together here we should just return a list of string and let the UI handle the putting together
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(towerDescription);
        foreach (var component in _advancedComponents)
        {
            sb.AppendLine(component.Description(stats));
        }
        return sb.ToString();
    } */

    public Dictionary<TowerStats, float> UpdateStats(Dictionary<TowerStats, float>  baseStats)
    {
        if( _isAmpDirty)
        {
            _amplificationProvider.Clear();
            foreach (var component in Components.Select(x => x.StatAlteration).Where(x => x is not null))
            {
                foreach(var (stat , a , m) in component.Modifications)
                {
                    var (ca, cm) = _amplificationProvider.GetValueOrDefault(stat,(0,1));
                    _amplificationProvider[stat] = (ca + a, cm * m);
                }
            }
            _isAmpDirty = false;
            float flat = 0;
            float mult = 0;
            cache.Clear();
            var allKeys = baseStats.Keys.Union(_amplificationProvider.Keys);

            foreach (var key in allKeys)
            {
                (flat, mult) = _amplificationProvider.GetValueOrDefault(key,(0,1));
                var value = baseStats.GetValueOrDefault(key, 0);
                cache[key] = (value + flat) * mult;
            }
        }
        return cache;
    }

    public List<Func<Dictionary<TowerStats, float>, Enemy, IEnumerator>> GetAttackAlteration()
    {
        return Components.Select(x => x.AdvancedAttackAlteration?.AttackFactory).Where(x => x is not null).ToList(); // this may need a sort later
    }

    public Func<List<Entity>,List<Entity>,List<Entity>> GetTargettingAletration()
    {
        return (x, y) => { foreach (var f in Components.Select(x => x.AdvancedTargettingAlteration?.RePrioritize).Where(x => x is not null)) { y = f(x, y); }; return y; };
    }

    public bool AddComponent(TowerComponent component)
    {
        if (Components.Contains(component))
            return false;

        // Find the first available place
        Vector2Int? componentSize = component.Size;

        if (componentSize == null)
            throw new ArgumentNullException("Size is null, component can't be auto placed.");

        for (int x = 0; x <= Size.x - componentSize.Value.x; x++)
        {
            for (int y = 0; y <= Size.y - componentSize.Value.y; y++)
            {
                if (Placeable(component, x, y))
                {
                    AddAt(component, x, y);
                    return true;
                }
            }
        }

        return false;
    }

    public bool AddComponent(TowerComponent component, Vector2Int at) => AddComponent(component, at.x, at.y);

    public bool AddComponent(TowerComponent component, int x, int y)
    {
        if (!Placeable(component, x, y) || Components.Contains(component))
            return false;

        AddAt(component, x, y);
        return true;
    }

    public bool RemoveComponent(TowerComponent component)
    {
        if (!Components.Contains(component))
            return false;

        foreach (Vector2Int c in component.Shape)
        {
            int x = component.position.x + c.x;
            int y = component.position.y + c.y;

            if (x < 0 || x >= Size.x ||
                y < 0 || y >= Size.y)
                continue;

            grid[x, y] = null;
        }

        _isAmpDirty = true;
        Components.Remove(component);

        return true;
    }

    public bool Placeable(Vector2Int at) => Placeable(at.x, at.y);

    public bool Placeable(int x, int y)
    {
        if (x < 0 || x >= Size.x ||
            y < 0 || y >= Size.y)
            return false;

        return grid[x, y] == null;
    }

    public bool Placeable(TowerComponent component, Vector2Int at) => Placeable(component, at.x, at.y);

    public bool Placeable(TowerComponent component, int x, int y)
    {
        foreach (Vector2Int c in component.Shape)
        {
            if (!Placeable(x + c.x, y + c.y))
                return false;
        }

        return true;
    }

    private void AddAt(TowerComponent component, Vector2Int at) => AddAt(component, at.x, at.y);

    private void AddAt(TowerComponent component, int x, int y)
    {
        foreach (Vector2Int c in component.Shape)
        {
            grid[x + c.x, y + c.y] = component;
        }

        component.position = new Vector2Int(x, y);
        Components.Add(component);
        _isAmpDirty = true;
    }
}
