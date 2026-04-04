using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    public static TowerManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            towers = new List<Tower>();
        }
        else
            Destroy(this);
    }

    private List<Tower> towers;

    public List<Tower> Towers
    {
        get => new List<Tower>(towers);
    }

    public void AddTower(Tower e) => towers.Add(e);

    public void RemoveTower(Tower e) => towers.Remove(e);
}
