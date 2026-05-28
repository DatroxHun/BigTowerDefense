using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class Road : MonoBehaviour
{
    [SerializeField] private int resolution = 100;
    [field: SerializeField] public float Width { get; private set; } = 0.8f;

    [field: SerializeField] public SplineContainer SplineContainer { get; private set; }
    [field: SerializeField] public LineRenderer LineRenderer { get; private set; }

    public float Length { get; private set; }
    public Vector3[] Positions
    {
        get
        {
            if (LineRenderer.positionCount == 0) return new Vector3[0];

            Vector3[] poses = new Vector3[LineRenderer.positionCount];
            LineRenderer.GetPositions(poses);
            return poses;
        }
    }

    void Start()
    {
        LineRenderer.startWidth = LineRenderer.endWidth = Width;

        LineRenderer.positionCount = resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            LineRenderer.SetPosition(i, EvaluatePosition(t));
        }

        Length = SplineContainer.Spline.GetLength();
        GenerateCollider();
    }

    public Vector3 EvaluatePosition(float t)
    {
        return SplineContainer.transform.TransformPoint(SplineContainer.Spline.EvaluatePosition(t));
    }

    public Vector3 EvaluateUpVector(float t)
    {
        Vector3 tangent = SplineContainer.Spline.EvaluateTangent(t);
        tangent.Normalize();
        return new Vector3(tangent.y, -tangent.x, 0);
    }

    /// <summary>
    /// Outputs the closest point on the road to the specified point in the spline's local coord system.
    /// </summary>
    /// <param name="localPos">specified point in the local coord system which from distance is measured</param>
    /// <param name="closestLocalPos">out: closest point to spec. point on spline</param>
    /// <param name="t">out: normalized t parameter of closestLocalPos on spline</param>
    public void GetClosestLocalPoint(float3 localPos, out float3 closestLocalPos, out float t)
    {
        SplineUtility.GetNearestPoint(SplineContainer.Spline, localPos, out closestLocalPos, out t);
    }

    public float Dist2T(float dist)
    {
        return SplineContainer.Spline.ConvertIndexUnit(dist, PathIndexUnit.Distance, PathIndexUnit.Normalized);
    }

    public float T2Dist(float t)
    {
        return SplineContainer.Spline.ConvertIndexUnit(t, PathIndexUnit.Normalized, PathIndexUnit.Distance);
    }

    public void GenerateCollider()
    {
        EdgeCollider2D edgeCollider = gameObject.GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
        {
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        }

        Vector3[] linePoints = new Vector3[LineRenderer.positionCount];
        LineRenderer.GetPositions(linePoints);

        List<Vector2> colliderPoints = new List<Vector2>();
        for (int i = 0; i < linePoints.Length; i++)
        {
            colliderPoints.Add(new Vector2(linePoints[i].x - this.transform.localPosition[0], linePoints[i].y - this.transform.localPosition[1]));
        }
        Debug.Log($"{SplineContainer.transform.localPosition[0] + this.transform.localPosition[0]} , {SplineContainer.transform.localPosition[1] + this.transform.localPosition[1]}");
        edgeCollider.SetPoints(colliderPoints);

        edgeCollider.edgeRadius = LineRenderer.startWidth / 3f; // tweak this if you want a smaller or bigger collider
    }
}
