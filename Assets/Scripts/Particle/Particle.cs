using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using System.Linq;

public class Particle : MonoBehaviour, IPoolable
{
    public GameObject Object => gameObject;
    public IObjectPool<IPoolable> Pool { get; set; }

    private int particleIndex = 0;
    [SerializeField] private ParticleSystem[] pSystems = null!;

    public void Return2Pool()
    {
        Pool.Release(this);
    }

    public void SpawnAction(Vector3 position)
    {
        transform.position = position;

        if (particleIndex > 0 && particleIndex < pSystems.Length)
            pSystems[particleIndex].Play();

        StartCoroutine(ReturnCondition());
    }

    public void SetParticleType(ParticleType type)
    {
        particleIndex = (int)type;
    }

    IEnumerator ReturnCondition()
    {
        yield return new WaitWhile(() => pSystems.Any(x => x.isPlaying));

        Return2Pool();
    }

    private void Start()
    {
        pSystems = GetComponentsInChildren<ParticleSystem>();
    }
}

public enum ParticleType
{
    Smoke,
    Divine,
}