using UnityEngine;

[ExecuteAlways]
public class FakeLiquidBottle : MonoBehaviour
{
    [SerializeField, Range(0,1)] private float m_fillPercent;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Collider m_collider;
    [Space] [SerializeField] private Vector3 m_normal;

    [Space]
    [SerializeField] float MaxWobble = 0.03f;
    [SerializeField] float WobbleSpeedMove = 1f;
    [SerializeField] float Recovery = 1f;
    [SerializeField] float Thickness = 1f;


    private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");

    private float time;
    private float pulse;
    private float sinewave;
    float wobbleAmountX;
    float wobbleAmountZ;
    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;
    private Vector3 velocity;
    private Vector3 angularVelocity;
    private Vector3 lastPos;
    private Quaternion lastRot;

    private Vector3 m_lastPosition;
    private Vector3 m_lastRotation;

    private Vector3 m_positionDiff;
    private Vector3 m_rotationDiff;
    private Vector3 m_flow;

    private static readonly int Normal = Shader.PropertyToID("_Normal");
    // private Vector3 m_velocity;

    // For convenience let' leave the Update method
    private void Update()
    {
        UpdateFillPosition();
        UpdateWobble2(m_meshRenderer.transform, Time.deltaTime);
    }

    private void UpdateWobble()
    {
        float deltaTime = Time.deltaTime;
        time += Time.deltaTime;

        // decrease wobble over time
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, deltaTime * Recovery);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, deltaTime * Recovery);

        // make a sine wave of the decreasing wobble
        pulse = 2 * Mathf.PI * WobbleSpeedMove;
        sinewave = Mathf.Lerp(sinewave, Mathf.Sin(pulse * time),
            deltaTime * Mathf.Clamp(velocity.magnitude + angularVelocity.magnitude, Thickness, 10));

        wobbleAmountX = wobbleAmountToAddX * sinewave;
        wobbleAmountZ = wobbleAmountToAddZ * sinewave;

        // velocity
        velocity = (lastPos - m_meshRenderer.transform.position) / deltaTime;

        angularVelocity = GetAngularVelocity(lastRot, m_meshRenderer.transform.rotation);

        // add clamped velocity to wobble
        wobbleAmountToAddX +=
            Mathf.Clamp((velocity.x + (velocity.y * 0.2f) + angularVelocity.z + angularVelocity.y) * MaxWobble,
                -MaxWobble, MaxWobble);
        wobbleAmountToAddZ +=
            Mathf.Clamp((velocity.z + (velocity.y * 0.2f) + angularVelocity.x + angularVelocity.y) * MaxWobble,
                -MaxWobble, MaxWobble);

        // send it to the shader
        m_meshRenderer.material.SetFloat("_WobbleX", wobbleAmountX);
        m_meshRenderer.material.SetFloat("_WobbleZ", wobbleAmountZ);

        // keep last position
        lastPos = m_meshRenderer.transform.position;
        lastRot = m_meshRenderer.transform.rotation;
    }

    private void UpdateWobble2(Transform transform, float deltaTime)
    {
        m_positionDiff = transform.position - m_lastPosition;
        m_rotationDiff = transform.rotation.eulerAngles - m_lastRotation;

        m_flow += m_positionDiff + m_rotationDiff;

        var xLerp = Mathf.Lerp(m_flow.x, 0, 0.2f);
        var yLerp = Mathf.Lerp(m_flow.y, 0, 0.2f);
        var zLerp = Mathf.Lerp(m_flow.z, 0, 0.2f);
        m_flow = new Vector3(xLerp, yLerp, zLerp);

        // Quaternion.Lerp()

        // m_meshRenderer.material.SetColor("_Color", new Color(m_flow.x, m_flow.y, m_flow.z, 1));

        // Debug.LogError(m_positionDiff.ToString());
        m_lastPosition = transform.position;
    }
    
    //https://forum.unity.com/threads/manually-calculate-angular-velocity-of-gameobject.289462/#post-4302796
    Vector3 GetAngularVelocity(Quaternion foreLastFrameRotation, Quaternion lastFrameRotation)
    {
        var q = lastFrameRotation * Quaternion.Inverse(foreLastFrameRotation);
        // no rotation?
        // You may want to increase this closer to 1 if you want to handle very small rotations.
        // Beware, if it is too close to one your answer will be Nan
        if (Mathf.Abs(q.w) > 1023.5f / 1024.0f)
            return Vector3.zero;
        float gain;
        // handle negatives, we could just flip it but this is faster
        if (q.w < 0.0f)
        {
            var angle = Mathf.Acos(-q.w);
            gain = -2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
        }
        else
        {
            var angle = Mathf.Acos(q.w);
            gain = 2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
        }

        Vector3 angularVelocity = new Vector3(q.x * gain, q.y * gain, q.z * gain);

        if (float.IsNaN(angularVelocity.z))
        {
            angularVelocity = Vector3.zero;
        }

        return angularVelocity;
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

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetVector(FillAmount, fillPos);
        m_meshRenderer.SetPropertyBlock(materialPropertyBlock);
        m_meshRenderer.sharedMaterial.SetVector(Normal, m_normal.normalized);
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
