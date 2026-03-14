#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Road road;
    [SerializeField] private SpawnObject spawnObject;

    public int CurrentWave { get; private set; } = 0;

    public event Action ToggleNextWave = null!;
    public Dictionary<int, ObjectPool<IPoolable>> objectPools { get; set; } = new();


    private Coroutine? waveCoroutine = null;


    void Start()
    {
        InitializePools();

        ToggleNextWave += () =>
        {
            if (waveCoroutine != null) return;

            waveCoroutine = StartCoroutine(ExecuteWave(CurrentWave++));
        };
    }

    private void InitializePools()
    {
        objectPools.Clear();

        foreach (Wave wave in spawnObject.waves)
        {
            foreach (Batch batch in wave.batches)
            {
                if (!objectPools.ContainsKey(batch.enemy.GetHashCode()))
                {
                    ObjectPool<IPoolable> newPool = null!;

                    newPool = new ObjectPool<IPoolable>
                    (
                        createFunc: () =>
                        {
                            GameObject obj = Instantiate(batch.enemy);
                            obj.SetActive(false);

                            IPoolable poolable = obj.GetComponent<IPoolable>();
                            poolable.Pool = newPool;

                            return poolable;
                        },
                        actionOnGet: (obj) => obj.Object.SetActive(true),
                        actionOnRelease: (obj) => obj.Object.SetActive(false),
                        actionOnDestroy: (obj) => Destroy(obj.Object),
                        collectionCheck: true,
                        defaultCapacity: 20,
                        maxSize: 100
                    );

                    objectPools.Add(batch.enemy.GetInstanceID(), newPool);
                }
            }
        }
    }

    private IEnumerator ExecuteWave(int waveIdx)
    {
        IEnumerator ExecuteBatch(Batch batch)
        {
            for (int i = 0; i < batch.amount; i++)
            {
                IPoolable pooled = objectPools[batch.enemy.GetInstanceID()].Get();
                pooled.SpawnAction(road.SplineContainer.gameObject.transform.position);

                if (batch.burstDelay > 0f)
                    yield return new WaitForSeconds(batch.burstDelay);
            }
        }

        // Start
        Debug.Log($"Starting Wave: {waveIdx}");
        Wave wave = spawnObject.waves[waveIdx];

        // Initial Delay
        yield return new WaitForSeconds(wave.delay);

        // Batch Execution
        foreach (Batch batch in wave.batches)
        {
            StartCoroutine(ExecuteBatch(batch));

            yield return new WaitForSeconds(batch.duration);
        }

        waveCoroutine = null;     
    }
}
