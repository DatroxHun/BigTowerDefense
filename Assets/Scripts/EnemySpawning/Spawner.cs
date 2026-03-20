#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Road road = null!;
    [SerializeField] private SpawnObject spawnObject = null!;

    [SerializeField] private float spawnRadius = .1f;

    public int CurrentWave { get; private set; } = 0;

    public event System.Action ToggleNextWave = null!;
    public Dictionary<int, ObjectPool<IPoolable>> objectPools { get; set; } = new();


    private Coroutine? waveCoroutine = null;


    void Start()
    {
        InitializePools();

        ToggleNextWave += () =>
        {
            // if wave is ongoing, do nothing
            if (waveCoroutine != null) return;

            waveCoroutine = StartCoroutine(ExecuteWave(CurrentWave++));
        };

        ToggleNextWave.Invoke();
    }

    private void InitializePools()
    {
        objectPools.Clear();

        foreach (Wave wave in spawnObject.waves)
        {
            foreach (Batch batch in wave.batches)
            {
                if (!objectPools.ContainsKey(batch.enemy.GetInstanceID()))
                {
                    ObjectPool<IPoolable> newPool = null!;

                    newPool = new ObjectPool<IPoolable>
                    (
                        createFunc: () =>
                        {
                            GameObject obj = Instantiate(batch.enemy.gameObject);
                            obj.SetActive(false);

                            if (!obj.TryGetComponent<Enemy>(out Enemy enemy))
                                throw new MissingComponentException("Spawner: Enemy component is missing from enemy.");

                            enemy.SetRoad(road);

                            if (!obj.TryGetComponent<IPoolable>(out IPoolable poolable))
                                throw new MissingComponentException("Spawner: IPoolable component is missing from enemy.");
                            
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
                Vector3 offset = (Vector3)Random.insideUnitCircle * spawnRadius;
                pooled.SpawnAction(road.SplineContainer.gameObject.transform.position + offset);

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
