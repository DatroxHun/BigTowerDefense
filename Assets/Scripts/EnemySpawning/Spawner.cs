#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Road road = null!;
    [SerializeField] private SpawnObject spawnObject = null!;

    [SerializeField] private Button nextWaveButton = null!;

    [SerializeField] private float spawnRadius = .1f;

    // wave variables
    public int CurrentWave { get; private set; } = 0;
    private static int waveDeposit = 0;
    private Coroutine? waveCoroutine = null;
    public static bool waveOnGoing { get => EnemyManager.instance.Enemies.Any(x => x.IsAlive) || spawners.Any(y => y.waveCoroutine != null); }

    // object pools
    public Dictionary<int, ObjectPool<IPoolable>> ObjectPools { get; private set; } = new();

    // static variables
    public static Spawner mainSpawner = null!;
    public static List<Spawner> spawners = new();
    public static event System.Action WaveStarted = null!;
    public static event System.Action WaveEnded = null!;



    void Start()
    {
        // append spawners list
        spawners.Add(this);

        // select main spawner
        if (mainSpawner == null)
        {
            mainSpawner = this;

            // MAIN SPAWNER INITIALIZATION: event handling
            WaveStarted += () =>
            {
                // check if wave ongoing -> return
                if (waveOnGoing || spawners.All(x => x.CurrentWave >= x.spawnObject.waves.Count))
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

                // global sideeffects
                AudioManager.PlayBGM(Clip.BattleBGM);
                nextWaveButton.interactable = false;
            };

            WaveEnded += () =>
            {
                // global sideeffects
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

        // NORMAL AND MAIN SPAWNER INITS: init object pools
        InitializePools();        
    }

    public void StartWave() => WaveStarted?.Invoke();

    private void InitializePools()
    {
        ObjectPools.Clear();

        // iterate through all enemies present in spawnObject
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
                            // instantiate object
                            GameObject obj = Instantiate(batch.enemy.gameObject);
                            obj.transform.parent = EnemyManager.instance.gameObject.transform;
                            obj.SetActive(false);

                            // set road to follow
                            if (!obj.TryGetComponent<Enemy>(out Enemy enemy))
                                throw new MissingComponentException("Spawner: Enemy component is missing from enemy.");

                            enemy.SetRoad(road);

                            // set parent pool
                            if (!obj.TryGetComponent<IPoolable>(out IPoolable poolable))
                                throw new MissingComponentException("Spawner: IPoolable component is missing from enemy.");
                            
                            poolable.Pool = newPool;

                            return poolable;
                        },

                        actionOnGet: (obj) =>
                        {
                            obj.Object.SetActive(true);

                            // track live enemies in enemy manager
                            if (!obj.Object.TryGetComponent<Enemy>(out Enemy enemy))
                                throw new MissingComponentException("Spawner: Enemy component is missing from enemy.");
                            else
                                EnemyManager.instance.AddEnemy(enemy);
                        },

                        actionOnRelease: (obj) =>
                        {
                            obj.Object.SetActive(false);

                            // untrack dead enemies in enemy manager
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

                    // add new object pool to dictionary refered to by the spawned enemy type as key
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

                // call spawn action with desired position
                Vector3 offset = (Vector3)Random.insideUnitCircle * spawnRadius;
                //Vector3 spawnPos = road.SplineContainer.gameObject.transform.position + offset;
                Vector3 spawnPos = transform.position + offset;
                spawnPos.z = 0;
                pooled.SpawnAction(spawnPos);

                if (batch.burstDelay > 0f)
                    yield return new WaitForSeconds(batch.burstDelay);
            }
        }

        // start
        Debug.Log($"Starting Wave: {waveIdx}");
        Wave wave = spawnObject.waves[waveIdx];

        // initial delay
        yield return new WaitForSeconds(wave.delay);

        // batch execution
        foreach (Batch batch in wave.batches)
        {
            StartCoroutine(ExecuteBatch(batch));

            yield return new WaitForSeconds(batch.duration);
        }

        // signal that this Spawner is finished
        waveDeposit += wave.reward;
        waveCoroutine = null;

        // only end waves if this is the last spawner
        if (spawners.All(x => x.waveCoroutine == null))
        {
            // wait until all enemies are dead
            yield return new WaitWhile(() => EnemyManager.instance.Enemies.Any(x => x.IsAlive));

            // call end wave
            WaveEnded?.Invoke();
        }
    }

    private void OnDestroy()
    {
        // all spawners
        spawners.Remove(this);

        // if this was the main spawner, clean up the global state
        if (mainSpawner == this)
        {
            mainSpawner = null!;
            spawners.Clear();

            // clear all event subscribers so dead scripts don't listen
            WaveStarted = null!;
            WaveEnded = null!;
        }
    }
}
