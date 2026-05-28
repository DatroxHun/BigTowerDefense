#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Road road = null!;
    [SerializeField] private SpawnObject spawnObject = null!;

    [SerializeField] private Button nextWaveButton = null!;

    [SerializeField] private float spawnRadius = .1f;

    public int CurrentWave { get; private set; } = 0;

    public static Spawner mainSpawner = null!;
    public static List<Spawner> spawners = new();
    public static event System.Action WaveStarted = null!;
    public static event System.Action WaveEnded = null!;

    public Dictionary<int, ObjectPool<IPoolable>> ObjectPools { get; private set; } = new();


    private Coroutine? waveCoroutine = null;
    public static bool waveOnGoing { get => EnemyManager.instance.Enemies.Any(x => x.IsAlive) || spawners.Any(y => y.waveCoroutine != null); }
    private static int waveDeposit = 0;

    void Start()
    {
        spawners.Add(this);

        if (mainSpawner == null)
        {
            // ONLY IF THIS IS MAIN SPAWNER
            mainSpawner = this;

            WaveStarted += () =>
            {
                // check if wave ongoing -> return
                if (waveOnGoing)
                    return;

                // heal towers
                TowerManager.instance.RepairTowers();

                // start spawner waves
                foreach (Spawner swr in spawners)
                {
                    if (swr.CurrentWave < swr.spawnObject.waves.Count)
                    {
                        swr.waveCoroutine = swr.StartCoroutine(swr.ExecuteWave(swr.CurrentWave++));
                    }
                }

                // reset deposit
                waveDeposit = 0;

                // global stuff
                AudioManager.PlayBGM(Clip.BattleBGM);
                nextWaveButton.interactable = false;
            };

            WaveEnded += () =>
            {
                // global stuff
                AudioManager.PlayBGM(Clip.CalmBGM);
                nextWaveButton.interactable = true;

                // check win condition
                if (spawners.All(x => x.spawnObject.waves.Count <= x.CurrentWave))
                {
                    GameOver.Instance.Toggle(won: true);
                }
                else
                {
                    // only heal if not end of the game
                    TowerManager.instance.RepairTowers();
                }

                // realize reward
                BuildingManager.instance.Resources += waveDeposit;
                waveDeposit = 0;

                Debug.Log("Wave Ended");
            };
        }

        // NORMAL AND MAIN SPAWNER INITS
        InitializePools();        
    }

    public void StartWave()
    {
        if (!waveOnGoing && spawners.Any(x => x.CurrentWave < x.spawnObject.waves.Count))
            WaveStarted?.Invoke();
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

        // Signal that this Spawner is finished
        waveDeposit += wave.reward;
        waveCoroutine = null;

        // I am the last spawner to finish
        if (spawners.All(x => x.waveCoroutine == null))
        {
            // Wait until all enemies are dead
            yield return new WaitWhile(() => EnemyManager.instance.Enemies.Any(x => x.IsAlive));

            // Call end wave
            WaveEnded?.Invoke();
        }
    }

    private void OnDestroy()
    {
        // all spawners
        spawners.Remove(this);

        // If this was the main spawner, clean up the global state
        if (mainSpawner == this)
        {
            mainSpawner = null!;
            spawners.Clear();

            // Clear all event subscribers so dead scripts don't listen
            WaveStarted = null!;
            WaveEnded = null!;
        }
    }
}
