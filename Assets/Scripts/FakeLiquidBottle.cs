using UnityEngine;

[ExecuteAlways]
public class FakeLiquidBottle : MonoBehaviour
{
    [SerializeField, Range(0,1)] private float m_fillPercent;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Collider m_collider;

    private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");

    // For convenience let' leave the Update method
    private void Update()
    {
        UpdateFillPosition();
    }

    private void UpdateFillPosition()
    {
        Bounds bounds = m_collider.bounds;

        // Get the fill position in the distance from center form
        float percentWorldPosY = Mathf.Lerp(bounds.min.y, bounds.max.y, m_fillPercent);
        Vector3 distanceFromBoundsCenter = new Vector3(0,bounds.center.y - percentWorldPosY, 0);

        // Fill position is determined by the distance from the pivot point to the point of liquid level
        // which itself is distance from the bounds point to the point of liquid level (only Y coordinate)
        Vector3 fillPos = bounds.center - distanceFromBoundsCenter - m_meshRenderer.transform.position;
        m_meshRenderer.sharedMaterial.SetVector(FillAmount, fillPos);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(m_meshRenderer.transform.TransformPoint(m_meshRenderer.localBounds.center), m_meshRenderer.localBounds.size);
        Gizmos.DrawWireCube(m_meshRenderer.bounds.center, m_meshRenderer.bounds.size);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(m_meshRenderer.transform.position, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(m_meshRenderer.bounds.min, 0.3f);
        Gizmos.DrawSphere(m_meshRenderer.bounds.max, 0.3f);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(m_meshRenderer.bounds.center, 0.3f);
    }
}
