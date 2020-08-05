using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Based on
/// - http://blog.three-eyed-games.com/2018/05/03/gpu-ray-tracing-in-unity-part-1/
/// - https://youtu.be/Cp5WWtMoeKg
/// </summary>
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public sealed class RayTracingMaster : MonoBehaviour
{
    [SerializeField]
    private ComputeShader rayTracingShader;

    [SerializeField]
    private Texture skyboxTexture;

    [SerializeField]
    private Light lightSource;

    [SerializeField]
    [Range(1, 10)]
    private int lightBounceLimit = 8;

    [SerializeField]
    [Range(0.0f, 40.0f)]
    private float blendFactor = 15.0f;

    private RenderTexture target;

    private Camera currentCamera;

    private uint currentSample = 0;
    private Material addMaterial;

    private ComputeBuffer _sphereBuffer;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        currentCamera = Camera.current;

        var spheres = CreateSpheres();
        SetUpSphereBuffer(spheres);

        SetShaderParameters();

        Render(source, destination);
    }

    private void OnEnable()
    {
        currentSample = 0;
    }

    private void OnDisable()
    {
        if (_sphereBuffer != null)
        {
            _sphereBuffer.Release();
        }
    }

    private void SetShaderParameters()
    {
        if (Application.isPlaying)
        {
            rayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        }
        else
        {
            rayTracingShader.SetVector("_PixelOffset", new Vector2(0.5f, 0.5f));
        }

        rayTracingShader.SetMatrix("_CameraToWorld", currentCamera.cameraToWorldMatrix);
        rayTracingShader.SetMatrix("_CameraInverseProjection", currentCamera.projectionMatrix.inverse);

        rayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);

        switch (lightSource.type)
        {
            case LightType.Directional:
            {
                var l = lightSource.transform.forward;
                rayTracingShader.SetVector("_Light", new Vector4(l.x, l.y, l.z, lightSource.intensity));
                rayTracingShader.SetBool("_IsPointLight", false);
                break;
            }

            case LightType.Point:
            {
                var l = lightSource.transform.position;
                rayTracingShader.SetVector("_Light", new Vector4(l.x, l.y, l.z, lightSource.intensity));
                rayTracingShader.SetBool("_IsPointLight", true);
                break;
            }

            default:
                throw new System.Exception("Light type not supported.");
        }

        rayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);

        rayTracingShader.SetInt("_LightBounceLimit", lightBounceLimit);

        rayTracingShader.SetFloat("_BlendFactor", blendFactor);
    }

    private void Render(RenderTexture source, RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the source and target textures
        rayTracingShader.SetTexture(0, "Source", source);
        rayTracingShader.SetTexture(0, "Result", target);

        // Dispatch the compute shader
        var threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        var threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        if (Application.isPlaying)
        {
            if (addMaterial == null)
            {
                addMaterial = new Material(Shader.Find("Hidden/AddShader"));
            }

            addMaterial.SetFloat("_Sample", currentSample++);
            Graphics.Blit(target, destination, addMaterial);
        }
        else
        {
            Graphics.Blit(target, destination);
        }
    }

    private void InitRenderTexture()
    {
        if (target == null || target.width != Screen.width || target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (target != null)
            {
                target.Release();
            }

            // Get a render target for Ray Tracing
            target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    private void SetUpSphereBuffer(List<RayTracingSphere> spheres)
    {
        if (_sphereBuffer != null)
        {
            _sphereBuffer.Release();
        }

        // Assign to compute buffer
        _sphereBuffer = new ComputeBuffer(spheres.Count, 40);
        _sphereBuffer.SetData(spheres);
    }

    private static List<RayTracingSphere> CreateSpheres()
    {
        var spheres = new List<RayTracingSphere>();

        var allBodies = FindObjectsOfType<SimulatedBody>().Where(b => !b.IgnoreInRayTracing);

        foreach (var body in allBodies)
        {
            spheres.Add(body.GetVisualSphere());
        }

        return spheres;
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            currentSample = 0;
            transform.hasChanged = false;
        }
    }
}
