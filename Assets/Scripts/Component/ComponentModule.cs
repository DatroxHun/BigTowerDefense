using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class ComponentModule
{
    private List<TowerComponent> components = new List<TowerComponent>();
    private bool _isAmpDirty = true;
    private Dictionary<TowerStats, float> cache = new();
    private Dictionary<TowerStats,(float,float)> _amplificationProvider = new Dictionary<TowerStats, (float, float)>();

    private Vector2Int size;
    private TowerComponent[,] grid;

    public ComponentModule(Vector2Int size)
    {
        this.size = size;
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
            foreach (var component in components.Select(x => x.StatAlteration).Where(x => x is not null))
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
        return components.Select(x => x.AdvancedAttackAlteration.AttackFactory).Where(x => x is not null).ToList(); // this may need a sort later
    }

    public Func<List<Entity>,List<Entity>,List<Entity>> GetTargettingAletration()
    {
        return (x, y) => { foreach (var f in components.Select(x => x.AdvancedTargettingAlteration.RePrioritize).Where(x => x is not null)) { y = f(x, y); }; return y; };
    }

    public bool AddComponent(TowerComponent component)
    {
        // Find the first available place
        Vector2Int? componentSize = component.Size;

        if (componentSize == null)
            throw new ArgumentNullException("Size is null, component can't be auto placed.");

        for (int x = 0; x <= size.x - componentSize.Value.x; x++)
        {
            for (int y = 0; y <= size.y - componentSize.Value.y; y++)
            {
                if (Placeable(x, y))
                {
                    AddAt(component, x, y);
                    return true;
                }
            }
        }

        return false;
    }

    public bool AddComponent(TowerComponent component, Vector2Int position)
    {
        if (!Placeable(position)) return false;

        AddAt(component, position);
        return true;
    }

    public bool RemoveComponent(TowerComponent component)
    {
        if (!components.Contains(component))
            return false;

        foreach (Vector2Int c in component.Shape)
        {
            grid[component.position.x + c.x, component.position.y + c.y] = null;
        }

        _isAmpDirty = true;
        components.Remove(component);

        return true;
    }

    public bool Placeable(Vector2Int at) => Placeable(at.x, at.y);

    public bool Placeable(int x, int y)
    {
        if (x < 0 || x >= size.x ||
            y < 0 || y >= size.y)
            return false;

        return grid[x, y] == null;
    }

    private void AddAt(TowerComponent component, Vector2Int at) => AddAt(component, at.x, at.y);

    private void AddAt(TowerComponent component, int x, int y)
    {
        foreach (Vector2Int c in component.Shape)
        {
            grid[x + c.x, y + c.y] = component;
        }

        component.position = new Vector2Int(x, y);
        components.Add(component);
        _isAmpDirty = true;
    }
}
