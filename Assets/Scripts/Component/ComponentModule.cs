using System;
using System.Collections.Generic;
using System.Text;

public abstract class AttackArchetype { }
public abstract class DefendArchetype { }
public abstract class TargetArchetype { } // I can't think of examples right now so this may not be needed
public class DefendAction { }
public class TargetAction { }

public class ComponentModule
{
    private List<AdvancedTowerComponent> _advancedComponents;
    private List<BasicTowerComponent> _basicComponents;
    private bool _isAmpDirty = true;
    private Dictionary<TowerStats,(float,float)> _amplificationProvider;

    public string GenerateDescription(String towerDescription, Dictionary<TowerStats, float> stats) // style later, or maybe instead of putting the string together here we should just return a list of string and let the UI handle the putting together
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(towerDescription);
        foreach (var component in _advancedComponents)
        {
            sb.AppendLine(component.Description(stats));
        }
        return sb.ToString();
    }

    public Dictionary<TowerStats,(float,float)> GetAmplification()
    {
        if( _isAmpDirty)
        {
            _amplificationProvider = new Dictionary<TowerStats, (float, float)>();
            foreach (var component in _advancedComponents)
            {
                foreach(var tstat in component.Stats)
                {
                    _amplificationProvider[tstat] = (0, 1);
                }
            }
            foreach (var component in _basicComponents)
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



}
