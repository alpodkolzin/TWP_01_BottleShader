using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class FakeLiquidBottle : MonoBehaviour
{
    [SerializeField, Range(0,1)] private float m_fillPercent;
    [SerializeField] private MeshRenderer m_meshRenderer;

    private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");
    private Vector3 m_boundsCenterWorldPos;
    private Vector3 m_fillPos;

    // For convenience let' leave the Update method
    private void Update()
    {
        UpdateFillPosition();
    }

    private void UpdateFillPosition()
    {
        // float minY = (m_meshRenderer.localBounds.min).y;
        // float maxY = (m_meshRenderer.localBounds.max).y;

        // Vector3 fillAmount = new Vector3(0, Mathf.Lerp(minY, maxY, m_fillPercent), 0);
        // Vector3 pos = transform.TransformPoint(fillAmount) - transform.position;

        // m_meshRenderer.sharedMaterial.SetVector(FillAmount, pos);


        //Get the center of LOCAL BOUNDS in the world position, that's how we don't fixed on the model pivot
        // m_boundsCenterWorldPos = m_meshRenderer.transform.TransformPoint(m_meshRenderer.localBounds.center);
        //
        
        
        
        // Расстояния от pivot точки до "уровня воды"
        // уровень воды определяется расстоянием от центральной точки bounds
        // var fillPosY = Mathf.Lerp(m_meshRenderer.bounds.min.y, m_meshRenderer.bounds.max.y, m_fillPercent);
        

        m_fillPos = (m_meshRenderer.bounds.center - new Vector3(0, m_fillPercent, 0)) - m_meshRenderer.transform.position;
        m_meshRenderer.sharedMaterial.SetVector(FillAmount, m_fillPos);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(m_meshRenderer.transform.TransformPoint(m_meshRenderer.localBounds.center), m_meshRenderer.localBounds.size);
        Gizmos.DrawWireCube(m_meshRenderer.bounds.center, m_meshRenderer.bounds.size);
        

        Gizmos.color = Color.red;
        // Gizmos.DrawSphere(m_boundsCenterWorldPos, 0.3f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(m_meshRenderer.transform.position, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(m_meshRenderer.bounds.min, 0.3f);
        Gizmos.DrawSphere(m_meshRenderer.bounds.max, 0.3f);
        
        Gizmos.DrawSphere(m_meshRenderer.transform.TransformPoint(m_meshRenderer.localBounds.min), 0.3f); 
        Gizmos.DrawSphere(m_meshRenderer.transform.TransformPoint(m_meshRenderer.localBounds.max), 0.3f); 
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(m_meshRenderer.bounds.center, 0.3f);
    }
}
