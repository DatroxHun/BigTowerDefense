using UnityEngine;

public interface IStatProvider
{
    public float GetStatValue(TowerStats statName); // we may also need one that returns object if things get complicated
}
