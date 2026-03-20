using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public interface IPoolable
{
    GameObject Object { get; }
    IObjectPool<IPoolable> Pool { get; set; }

    void SpawnAction(Vector3 position);
    void Return2Pool();
}