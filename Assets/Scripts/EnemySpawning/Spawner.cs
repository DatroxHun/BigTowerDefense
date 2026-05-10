#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Road road = null!;
    [SerializeField] private SpawnObject spawnObject = null!;

    [SerializeField] private float spawnRadius = .1f;

    public int CurrentWave { get; private set; } = 0;

    public event System.Action WaveStarted = null!;
    public event System.Action WaveEnded = null!;

    public Dictionary<int, ObjectPool<IPoolable>> ObjectPools { get; private set; } = new();


    private Coroutine? waveCoroutine = null;
    public bool waveOnGoing { get => waveCoroutine != null; }


    void Start()
    {
        InitializePools();

        WaveStarted += () =>
        {
            // if wave is ongoing, do nothing
            if (waveCoroutine != null) return;

            waveCoroutine = StartCoroutine(ExecuteWave(CurrentWave++));
        };

        WaveEnded += () => Debug.Log("Wave Ended");
    }

    public void StartWave()
    {
        if (!waveOnGoing && CurrentWave < spawnObject.waves.Count) WaveStarted?.Invoke();
    }

    private void InitializePools()
    {
        ObjectPools.Clear();

        foreach (Wave wave in spawnObject.waves)
        {
            foreach (Batch batch in wave.batches)
            {
                if (!ObjectPools.ContainsKey(batch.enemy.GetInstanceID()))
                {
                    ObjectPool<IPoolable> newPool = null!;

                    newPool = new ObjectPool<IPoolable>
                    (
                        createFunc: () =>
                        {
                            GameObject obj = Instantiate(batch.enemy.gameObject);
                            obj.transform.parent = EnemyManager.instance.gameObject.transform;
                            obj.SetActive(false);

                            if (!obj.TryGetComponent<Enemy>(out Enemy enemy))
                                throw new MissingComponentException("Spawner: Enemy component is missing from enemy.");

                            enemy.SetRoad(road);

                            if (!obj.TryGetComponent<IPoolable>(out IPoolable poolable))
                                throw new MissingComponentException("Spawner: IPoolable component is missing from enemy.");
                            
                            poolable.Pool = newPool;

                            return poolable;
                        },

                        actionOnGet: (obj) =>
                        {
                            obj.Object.SetActive(true);

                            if (!obj.Object.TryGetComponent<Enemy>(out Enemy enemy))
                                throw new MissingComponentException("Spawner: Enemy component is missing from enemy.");
                            else
                                EnemyManager.instance.AddEnemy(enemy);
                        },

                        actionOnRelease: (obj) =>
                        {
                            obj.Object.SetActive(false);
                            if (!obj.Object.TryGetComponent<Enemy>(out Enemy enemy))
                                throw new MissingComponentException("Spawner: Enemy component is missing from enemy.");
                            else
                                EnemyManager.instance.RemoveEnemy(enemy);
                        },
                        actionOnDestroy: (obj) => Destroy(obj.Object),
                        collectionCheck: true,
                        defaultCapacity: 100,
                        maxSize: 500
                    );

                    ObjectPools.Add(batch.enemy.GetInstanceID(), newPool);
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
                IPoolable pooled = ObjectPools[batch.enemy.GetInstanceID()].Get();
                Vector3 offset = (Vector3)Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = road.SplineContainer.gameObject.transform.position + offset;
                spawnPos.z = 0;
                pooled.SpawnAction(spawnPos);

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

        WaveEnded?.Invoke();
        waveCoroutine = null;     
    }
}
