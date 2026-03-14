using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class Road : MonoBehaviour
{
    [SerializeField] private int resolution = 100;

    [field: SerializeField] public SplineContainer SplineContainer { get; private set; }
    [field: SerializeField] public LineRenderer LineRenderer { get; private set; }

    public float Length { get; private set; }
    public Vector3[] Positions
    {
        get
        {
            Vector3[] poses = new Vector3[0];
            LineRenderer.GetPositions(poses);
            return poses;
        }
    }

    void Start()
    {
        LineRenderer.positionCount = resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            LineRenderer.SetPosition(i, SplineContainer.transform.position + (Vector3)SplineContainer.Spline.EvaluatePosition(t));
        }

        Length = SplineContainer.Spline.GetLength();
    }

    public Vector2 EvaluatePosition(float t)
    {
        float3 result = SplineContainer.Spline.EvaluatePosition(t);
        return new Vector2(result.x, result.y);
    }
}
