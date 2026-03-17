using UnityEngine;

public interface IStatProvider
{
    public float? GetStatValue(TowerStats statName); // we may also need one that returns object if things get complicated
    public void Merge(IStatProvider statstomerge);
    // public void UpdateStat(TowerStats statName, float basemodification, float multipliermodification); I am still not sure about the represetation
}
