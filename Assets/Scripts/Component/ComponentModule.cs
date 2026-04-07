using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class ComponentModule
{
    private List<TowerComponent> components = new List<TowerComponent>();
    private bool _isAmpDirty = true;
    private Dictionary<TowerStats,(float,float)> _amplificationProvider;

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

    public Dictionary<TowerStats,(float,float)> GetAmplification()
    {
        if( _isAmpDirty)
        {
            _amplificationProvider.Clear();
            foreach (var stats in components.Select(x => x.AdvancedAttackAlteration.Stats))
            {
                foreach (var tstat in stats)
                {
                    _amplificationProvider[tstat] = (0, 1);
                }
            }
            foreach (var component in components.Select(x => x.StatAlteration))
            {
                foreach(var (stat , a , m) in component.Modifications)
                {
                    var (ca, cm) = _amplificationProvider[stat];
                    _amplificationProvider[stat] = (ca + a, cm * m);
                }
            }
            _isAmpDirty = false;
        }
        return _amplificationProvider;
    }

    public List<Func<Dictionary<TowerStats, float>, Enemy, IEnumerator>> GetAttackAlteration()
    {
        return components.Select(x => x.AdvancedAttackAlteration.AttackFactory).Where(x => x is not null).ToList(); // this may need a sort later
    }

    public Func<List<Entity>,List<Entity>,List<Entity>> GetTargettingAletration()
    {
        return (x, y) => { foreach (var f in components.Select(x => x.AdvancedTargettingAlteration.RePrioritize).Where(x => x is not null)) { y = f(x, y); }; return y; };
    }

}
