#nullable enable

using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;

public class Walker : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public NavMeshAgent Agent { get; private set; } = null!;

    private Action<Action?> walkAction = null!;
    private Action? globalCallback = null;

    // Spline
    private Road? road = null;
    private float? distTraversed = null;
    private float roadSidewaysCoeff = 0.0f;

    // Coord
    private Vector3? destination = null;

    void Awake()
    {
        SetMode(WalkModes.Stop);
    }

    void Start()
    {
        Agent.enabled = true;

        Agent.speed = Speed;
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;
    }

    void Update()
    {
        // walkAction.Invoke(globalCallback);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Convert mouse position to world space
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0; // Ensure we stay on the 2D plane

            Agent.SetDestination(mousePos);
        }


    }


    public void SetRoad(Road road) => this.road = road;
    public void SetDestination(Vector3 dest) => destination = dest;

    public void SetMode(WalkModes mode, Action? globalCallback = null)
    {
        this.globalCallback = globalCallback;

        switch (mode)
        {
            case WalkModes.Road:
                walkAction = WalkOnRoad;
                break;
            case WalkModes.Coordinate:
                distTraversed = null;
                walkAction = Walk2Coord;
                break;
            case WalkModes.Stop:
                walkAction = Stop;
                break;
            default:
                walkAction = WalkOnRoad;
                break;
        }
    }


    private void WalkOnRoad(Action? callback = null)
    {
        // If no road, stop
        if (road == null)
        {
            SetMode(WalkModes.Stop);
            return;
        }

        // Initialize walk
        if (distTraversed is null)
        {
            // Get local coords of position in spline's coord system and get closest point on spline
            float3 localPos = road.SplineContainer.transform.InverseTransformPoint(transform.position);
            road.GetClosestLocalPoint(localPos, out float3 closestLocalPos, out float t);         
            
            // Get global coords of closest point and set distance traveled on spline accordingly
            destination = road.SplineContainer.transform.TransformPoint(closestLocalPos); // to global coord system            
            distTraversed = road.T2Dist(t); // get distance traversed on spline

            // Recalculate sideways-translation on road. Constant for an uninterrrupted road-traversing process
            roadSidewaysCoeff = Random.Range(-.2f, .2f) * road.Width;
        }

        // Check if arrived
        float currentT = road.Dist2T(distTraversed.Value);
        if (currentT >= 1f - 1e-3f) // End of Spline
        {
            // Set distTraversed to null, showing that no longer "attached" to spline
            distTraversed = null;          
            SetMode(WalkModes.Stop);

            callback?.Invoke();
            return;
        }

        // Walk to current destination, then update point
        Walk2Coord(() =>
        {
            // Calculate next point on spline
            distTraversed += Random.Range(0.5f, 1.0f); // arbitrary step size
            float t = road.Dist2T(distTraversed.Value);

            // Add random offset
            Vector3 localUp = road.EvaluateUpVector(t);
            destination = road.EvaluatePosition(t) + localUp * roadSidewaysCoeff; // can add per-step perturbance here as a randomly generated multiplyer

            // Continue walking on road with same callback
            SetMode(WalkModes.Road, callback);
        });
    }

    private void Walk2Coord(Action? callback = null)
    {
        // If no destination, stop
        if (destination == null)
        {
            SetMode(WalkModes.Stop);
            return;
        }

        float step = Speed * Time.deltaTime;
        Vector3 target = destination.Value;

        // Check if arrived
        if (Vector3.Distance(transform.position, target) <= step)
        {
            transform.position = target;

            // Set destination to null, showing that no longer towards coord
            destination = null;
            SetMode(WalkModes.Stop);

            callback?.Invoke();
            return;
        }

        // Otherwise move towards coord
        transform.position = Vector3.MoveTowards(transform.position, target, step);
    }

    private void Stop(Action? callback = null)
    {
        // clear globalCallback so it only runs once
        globalCallback = null;
        callback?.Invoke();
    }
}

public enum WalkModes
{
    Road,
    Coordinate,
    Stop
}