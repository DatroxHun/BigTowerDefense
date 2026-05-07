using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Splines.ExtrusionShapes;

public class ParticlePool : MonoBehaviour
{
    public static ParticlePool instance;

    [SerializeField] private GameObject particlePrefab;

    private ObjectPool<IPoolable> pool = null!;

    private void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        pool = new ObjectPool<IPoolable>
        (
            createFunc: () =>
            {
                GameObject obj = Instantiate(particlePrefab);
                obj.transform.parent = this.transform;
                obj.SetActive(false);             

                if (!obj.TryGetComponent<IPoolable>(out IPoolable poolable))
                    throw new MissingComponentException("ParticlePool: IPoolable component is missing from prefab.");

                poolable.Pool = pool;

                return poolable;
            },
            actionOnGet: (obj) =>
            {
                obj.Object.SetActive(true);
            },
            actionOnRelease: (obj) =>
            {
                obj.Object.SetActive(false);
            },
            actionOnDestroy: (obj) => Destroy(obj.Object),
            collectionCheck: true,
            defaultCapacity: 50,
            maxSize: 500
        );
    }

    public void Emit(Vector3 position, ParticleType type)
    {
        IPoolable poolable = pool.Get();

        if (!poolable.Object.TryGetComponent<Particle>(out Particle particle))
            throw new MissingComponentException("ParticlePool: Particle component is missing from prefab.");

        particle.SetParticleType(type);
        particle.SpawnAction(position);
    }
}
