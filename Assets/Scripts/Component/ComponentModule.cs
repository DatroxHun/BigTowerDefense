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

    public void AddComponent(TowerComponent component)
    {
        components.Add(component);
        _isAmpDirty = true;
    }

    public void RemoveComponent(TowerComponent component)
    {
        components.Remove(component);
        _isAmpDirty = true;
    }
}
