using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : Entity, IPoolable
{
    [SerializeField] private Walker walker;
    private Road road;

    public GameObject Object => gameObject;

    public IObjectPool<IPoolable> Pool { get; set; }

    public void Return2Pool() => Pool.Release(this);

    public void SpawnAction(Vector3 position)
    {
        transform.position = position;
        walker.SetRoad(road);
        walker.SetMode(WalkModes.Road, globalCallback: Return2Pool);
    }

    public void SetRoad(Road road) => this.road = road;

    protected override void Action() { }

    protected override void Target() { }
}
