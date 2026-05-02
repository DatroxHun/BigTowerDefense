using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class Particle : MonoBehaviour, IPoolable
{
    public GameObject Object => gameObject;

    public IObjectPool<IPoolable> Pool { get; set; }

    [SerializeField] private ParticleSystem pSystem = null!;

    public void Return2Pool()
    {
        Pool.Release(this);
    }

    public void SpawnAction(Vector3 position)
    {
        transform.position = position;
        pSystem.Play();

        StartCoroutine(ReturnCondition());
    }

    IEnumerator ReturnCondition()
    {
        yield return new WaitWhile(() => pSystem.isPlaying);

        Return2Pool();
    }

    private void Start()
    {
        pSystem = GetComponent<ParticleSystem>();
    }
}
