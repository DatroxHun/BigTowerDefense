using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    public void Awake()
    {
        if (instance is null)
        {
            instance = this;
            enemies = new List<Enemy>();
        }
        else
            Destroy(this);
    }

    private List<Enemy> enemies;

    public List<Enemy> Enemies
    {
        get { return new List<Enemy>(enemies); }
    }



    public void AddEnemy(Enemy e) => enemies.Add(e);

    public void RemoveEnemy(Enemy e) => enemies.Remove(e);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
