using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class Road : MonoBehaviour
{
    [SerializeField] private int resolution = 100;

    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private LineRenderer lineRenderer;

    public float Length { get; private set; }
    public Vector3[] Positions
    {
        get
        {
            Vector3[] poses = new Vector3[0];
            lineRenderer.GetPositions(poses);
            return poses;
        }
    }

    void Start()
    {
        lineRenderer.positionCount = resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            lineRenderer.SetPosition(i, splineContainer.transform.position + (Vector3)splineContainer.Spline.EvaluatePosition(t));
        }

        Length = splineContainer.Spline.GetLength();
    }

    public Vector2 EvaluatePosition(float t)
    {
        float3 result = splineContainer.Spline.EvaluatePosition(t);
        return new Vector2(result.x, result.y);
    }
}
