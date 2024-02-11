using UnityEngine;

[ExecuteAlways]
public class FakeLiquidBottle : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float m_fillPercent;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Collider m_collider;

    [SerializeField] private Vector3 GravityVector = Vector3.up;
    [Space, Header("Flow settings")]
    [SerializeField] private float FlowDumpingSpeed = 0.2f;

    [Space, Header("Wobble Settings")]
    [SerializeField] private float WobbleRecovery = 10;
    [SerializeField] private float WobbleStrength = 5;
    [SerializeField] private float WobbleDumpingSpeed = .1f;
    [SerializeField] private float WobbleIgnoreThreshold = 0.5f;

    private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");
    private static readonly int Normal = Shader.PropertyToID("_Normal");

    private Vector3 m_lastPosition;
    private Vector3 m_positionDiff;
    private Vector3 m_flow;

    private float m_wobbleDumpingValue;

    // For convenience let's leave the Update method
    private void Update()
    {
        UpdateWobble(m_meshRenderer.transform);
        UpdateFlowVector();
    }

    private void UpdateWobble(Transform transform)
    {
        m_positionDiff = transform.position - m_lastPosition;
        m_wobbleDumpingValue += m_positionDiff.magnitude;
        m_flow += m_positionDiff;

        m_wobbleDumpingValue = Mathf.Lerp(m_wobbleDumpingValue, 0, WobbleDumpingSpeed);
        m_flow = RotateFlowVector(m_flow, m_positionDiff.magnitude, m_wobbleDumpingValue);

        m_flow = ClampFlowVector(m_flow);

        m_lastPosition = transform.position;
    }

    private Vector3 RotateFlowVector(Vector3 flow, float positionDiffMagnitude, float wobbleDumpingValue)
    {
        if (!(Mathf.Abs(positionDiffMagnitude) < WobbleIgnoreThreshold))
        {
            return flow;
        }

        float angle = Mathf.Sin(Time.time * WobbleRecovery) * WobbleStrength * wobbleDumpingValue;
        Vector3 rotationAxis = Vector3.Cross(flow.normalized, GravityVector);

        // prevent cross product flipping
        if (Vector3.Dot(rotationAxis, new Vector3(1, 0, 1)) < 0.0f)
        {
            rotationAxis = -rotationAxis;
        }

        return Quaternion.AngleAxis(angle, rotationAxis) * flow;
    }

    private Vector3 ClampFlowVector(Vector3 flow)
    {
        Vector3 reducedFlow = Vector3.Slerp(flow, GravityVector, FlowDumpingSpeed);
        return reducedFlow;
    }

    private void UpdateFlowVector()
    {
        Bounds bounds = m_collider.bounds;

        // Get the fill position in the distance from center form
        float percentWorldPosY = Mathf.Lerp(bounds.min.y, bounds.max.y, m_fillPercent);
        Vector3 distanceFromBoundsCenter = new Vector3(0, bounds.center.y - percentWorldPosY, 0);

        // Fill position is determined by the distance from the pivot point to the point of liquid level
        // which itself is distance from the bounds point to the point of liquid level (only Y coordinate)
        Vector3 fillPos = bounds.center - distanceFromBoundsCenter - m_meshRenderer.transform.position;

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetVector(FillAmount, fillPos);
        materialPropertyBlock.SetVector(Normal, m_flow);
        m_meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void OnDrawGizmos()
    {
        Vector3 boundsCenter = m_meshRenderer.bounds.center;

        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(m_meshRenderer.transform.TransformPoint(m_meshRenderer.localBounds.center), m_meshRenderer.localBounds.size);
        Gizmos.DrawWireCube(boundsCenter, m_meshRenderer.bounds.size);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(m_meshRenderer.transform.position, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(m_meshRenderer.bounds.min, 0.3f);
        Gizmos.DrawSphere(m_meshRenderer.bounds.max, 0.3f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(boundsCenter, 0.3f);
        Gizmos.DrawLine(boundsCenter, boundsCenter + m_flow);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(boundsCenter, boundsCenter + Vector3.Cross(m_flow, Vector3.up).normalized);
    }
}