using UnityEngine;

/// <summary>
/// WARNING: If you change the size of this structure,
/// see how ComputeBuffer is created in RayTracingMaster.
/// </summary>
public struct RayTracingSphere
{
    public Vector3 position;
    public float radius;
    public Vector3 albedo;
    public Vector3 specular;
}
