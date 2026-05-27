using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Batch
{
    [Tooltip("Enemy to spawn")]
    public GameObject enemy;

    [Tooltip("Amount to spawn")]
    public int amount;

    [Tooltip("Time between each unit spawn (in sec)")]
    public float burstDelay;

    [Tooltip("Delay until next batch is spawn (in sec)")]
    public float duration;
}

[System.Serializable]
public struct Wave
{
    [TextArea]
    public string description;

    [Tooltip("Reward resouces after wave")]
    public int reward;

    [Tooltip("Initial delay (in sec)")]
    public float delay;

    [Tooltip("Sequential list of spawn batches")]
    public List<Batch> batches;
}

[CreateAssetMenu(fileName = "SpawnObject", menuName = "Scriptable Objects/SpawnObject")]
public class SpawnObject : ScriptableObject
{
    [Tooltip("Sequential list of spawn waves")]
    public List<Wave> waves = new();
}