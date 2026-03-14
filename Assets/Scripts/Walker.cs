#nullable enable

using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class Walker : MonoBehaviour
{
    [SerializeField] public float speed;

    private Road? road = null;
    private Vector3? destination = null;

    private Action<Action?> walkAction;


    // Spline
    private float? distTraversed = null;

    // Coord
    private Vector2? prevVel = null;


    void Start()
    {
        SetMode(WalkModes.Road);
    }

    void Update()
    {
        walkAction.Invoke(null);
    }


    public void SetRoad(Road road) => this.road = road;
    public void SetDestination(Vector3 dest) => destination = dest;

    public void SetMode(WalkModes mode)
    {
        switch (mode)
        {
            case WalkModes.Road:
                walkAction = WalkOnRoad;
                break;
            case WalkModes.Coordinate:
                walkAction = Walk2Coord;
                break;
            case WalkModes.Stop:
                walkAction = Stop;
                break;
            default:
                walkAction = WalkOnRoad;
                break;
        }

        if (mode != WalkModes.Coordinate)
            prevVel = null;
    }


    private void WalkOnRoad(Action? callback = null)
    {
        if (road == null)
        {
            SetMode(WalkModes.Stop);
            return;
        }

        if (destination is null)
        {
            // Find closest Point on Spline
            float3 localPos = road.SplineContainer.transform.InverseTransformPoint(transform.position); // to spline's local coords
            SplineUtility.GetNearestPoint(road.SplineContainer.Spline, localPos, out float3 nearestLocalPos, out float t); // get closest float3
            distTraversed = road.SplineContainer.Spline.ConvertIndexUnit(t, PathIndexUnit.Normalized, PathIndexUnit.Distance); // get distance traversed on spline

            // Walk to that point
            destination = (Vector3)nearestLocalPos;
            Walk2Coord(() =>
            {
                // Walk along the Spline

            });
        }

        distTraversed = null;
    }

    private void Walk2Coord(Action? callback = null)
    {
        if (destination == null)
        {
            SetMode(WalkModes.Stop);
            return;
        }

        Vector3 velocity = (destination.Value - gameObject.transform.position) * speed * Time.deltaTime;
        if (prevVel != null && Vector3.Dot(prevVel.Value, velocity) < 0f) // if the direction of velocity change -> arrived
        {
            transform.position = destination.Value;
            destination = null;
            SetMode(WalkModes.Stop);
            callback?.Invoke();
        }
        else // if velocity stays the same or just departed, move forward
        {
            transform.position += velocity;
            prevVel = velocity;
        }

    }
    
    private void Stop(Action? callback = null) { }
}

public enum WalkModes
{
    Road,
    Coordinate,
    Stop
}