#nullable enable

using System;
using System.Net;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Walker : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public NavMeshAgent Agent { get; private set; } = null!;

    public WalkModes Mode { get; private set; }
    private Action<Action?> walkAction = null!;
    private Action? globalCallback = null;

    // Spline
    private Road? road = null;
    private float? distTraversed = null;
    private float roadSidewaysCoeff = 0.0f;

    // Coord
    private Vector3? destination = null;
    private float? prevPosX = null, prevDX = null;


    void Awake()
    {
        SetMode(WalkModes.Stop);
    }

    void Start()
    {
        Agent.speed = Speed;
        Agent.avoidancePriority = Random.Range(0, 99);
        Agent.stoppingDistance = 0f;

        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        //OrientationChanged = new UnityEvent<float>();
        //OrientationChanged.AddListener(SelfListener);
    }
    /*
    private void SelfListener(float _)
    {
        Debug.Log("BBBBBBBBBBBBBBB");
    }
    */
    public UnityEvent<float> OrientationChanged { get; set; } = new UnityEvent<float>();

    //float prevDiffX, currentDiffX;
    void Update()
    {
        // walk
        walkAction.Invoke(globalCallback);

        // sprite flipping
        float dx = prevPosX.HasValue ? transform.position.x - prevPosX.Value : 0f;
        prevPosX = transform.position.x;

        if (!prevDX.HasValue || 
            (Mathf.Abs(dx) > 1e-8f && prevDX == 0) || 
            (Mathf.Abs(dx) > 1e-3f && dx * prevDX < 0f))
        {   
            OrientationChanged?.Invoke(dx);
        }
        prevDX = dx;




        //Debug.Log(Mode);

        //// DEBUG TOOL!!!
        //if (Mouse.current.leftButton.wasPressedThisFrame)
        //{
        //    // Convert mouse position to world space
        //    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //    mousePos.z = 0; // Ensure we stay on the 2D plane

        //    WalkOnPath(mousePos, 1f, () => Debug.Log("Arrived!"));
        //}


    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    #region Public Methods

    public void WalkOnRoad(Road? newRoad = null, Action? globalCallback = null)
    {
        SetRoad(newRoad);
        SetMode(WalkModes.Road, globalCallback);
    }

    public void Walk2Coord(Vector3 dest, Action? globalCallback = null)
    {
        SetDestination(dest);
        SetMode(WalkModes.Coordinate, globalCallback);
    }

    public void WalkOnPath(Vector3 dest, float radius, Action? globalCallback = null)
    {
        SetMode(WalkModes.Pathfind, globalCallback);
        SetPathEndPoint(dest, radius);    
    }

    public bool WalkOnPathRanged(Vector3 dest, float radius, LayerMask obstacleLayer, Action? globalCallback = null, 
                                 int numberOfSections = 6, int iterPerSection = 3)
    {
        SetMode(WalkModes.Pathfind, globalCallback);
        return SetPathEndPointRanged(dest, radius, obstacleLayer, numberOfSections, iterPerSection);
    }

    public void Stop(Action? globalCallback = null)
    {
        SetMode(WalkModes.Stop, globalCallback);
    }

    #endregion

    #region Private Methods

    private void SetRoad(Road? newRoad)
    {
        this.road = newRoad ?? this.road;
        distTraversed = null;
    }

    private void SetDestination(Vector3 dest) => destination = dest;

    private void SetPathEndPoint(Vector3 dest, float radius)
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        Vector3 endPoint = dest + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        Agent.SetDestination(endPoint);
    }

    private bool SetPathEndPointRanged(Vector3 target, float radius, LayerMask obstacleLayer, int numberOfSections, int iterPerSection)
    {
        float dy = this.transform.position.y - target.y;
        float dx = this.transform.position.x - target.x;
        float phi = Mathf.Atan2(dy, dx);

        for (int s = 1; s <= numberOfSections; s++)
        {
            float lowerDeltaPhi = (s - 1) * Mathf.PI / numberOfSections;
            float upperDeltaPhi = (s + 0) * Mathf.PI / numberOfSections;

            for (int i = 0; i < iterPerSection; i++)
            {
                float randomPhi = phi + Random.Range(lowerDeltaPhi, upperDeltaPhi) * (Random.value > 0.5 ? 1f : -1f);
                Vector3 potentialPosition = target + radius * new Vector3(Mathf.Cos(randomPhi), Mathf.Sin(randomPhi));

                RaycastHit2D result = Physics2D.Linecast(potentialPosition, target, obstacleLayer);
                if (result.collider == null)
                {
                    Agent.SetDestination(potentialPosition);
                    return true;
                }
            }
        }

        return false;
    }

    private void SetMode(WalkModes mode, Action? globalCallback = null)
    {
        Mode = mode;
        this.globalCallback = globalCallback;
        Agent.enabled = false;

        prevPosX = prevDX = null;

        switch (mode)
        {
            case WalkModes.Road:
                walkAction = WalkOnRoad;
                break;
            case WalkModes.Coordinate:
                distTraversed = null;
                walkAction = Walk2Coord;
                break;
            case WalkModes.Pathfind:
                Agent.enabled = true;
                walkAction = WalkOnPath;
                break;
            case WalkModes.Stop:
                walkAction = StopAction;
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
            roadSidewaysCoeff = Random.Range(-.5f, .5f) * road.Width * .4f;

            // If too far from road, find path to it first
            if (Vector3.Distance(transform.position, destination.Value) > road.Width / 2f)
            {
                WalkOnPath(destination.Value, 0f, () =>
                {
                    WalkOnRoad(road, callback);
                });

                return;
            }
        }

        // Check if arrived
        float currentT = road.Dist2T(distTraversed.Value);
        if (currentT >= 1f - 1e-4f) // End of Spline
        {
            // Set distTraversed to null, showing that no longer "attached" to spline
            distTraversed = null;
            SetMode(WalkModes.Stop);
            callback?.Invoke();

            return;
        }

        // Walk to current destination, then update point
        Walk2Coord(() =>
        //WalkOnPath(destination ?? Vector3.zero, 0f, () =>
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

    private void WalkOnPath(Action? callback = null)
    {
        // Check if arrived
        if (!Agent.pathPending)
        {
            if (Agent.remainingDistance <= Agent.stoppingDistance)
            {
                if (!Agent.hasPath || Agent.velocity.sqrMagnitude < 1e-2f)
                {
                    Agent.enabled = false;                    

                    SetMode(WalkModes.Stop);
                    callback?.Invoke();
                    
                    return;
                }
            }
        }
    }

    private void StopAction(Action? callback = null)
    {
        // clear globalCallback so it only runs once
        globalCallback = null;
        callback?.Invoke();
    }

    #endregion
}

public enum WalkModes
{
    Road,
    Coordinate,
    Pathfind,
    Stop
}