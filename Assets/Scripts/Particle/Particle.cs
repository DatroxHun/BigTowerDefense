using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using System.Linq;

public class Particle : MonoBehaviour, IPoolable
{
    public GameObject Object => gameObject;
    public IObjectPool<IPoolable> Pool { get; set; }

    float sizeMultiplier = 1;

    private int particleIndex = 0;
    [SerializeField] private ParticleSystem[] pSystems = null!;

    public void Return2Pool()
    {
        Pool.Release(this);
    }

    public void SpawnAction(Vector3 position)
    {
        transform.position = position;

        //transform.localScale = Vector3.one; // presumed to be default; needed for scaling

        if (particleIndex >= 0 && particleIndex < pSystems.Length)
        {
            ParticleSystem ps = pSystems[particleIndex];

            ps.transform.localScale = Vector3.one * sizeMultiplier;
            
            ps.Play();
        }

        StartCoroutine(ReturnCondition());
    }

    public void SetParticleType(ParticleType type)
    {
        particleIndex = (int)type;
    }

    public void ModifySize(float sizeMultiplier = 1)
    {
        this.sizeMultiplier = sizeMultiplier;
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
    ChiefEmission
}