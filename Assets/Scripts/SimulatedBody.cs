using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SimulatedBodyPrediction))]
public sealed class SimulatedBody : MonoBehaviour, ISimulatedBody
{
    [SerializeField]
    private Vector3 initialVelocity = Vector3.zero;

    [SerializeField]
    private bool drawOrbit = false;

    [SerializeField]
    private Color specularColor = new Color(0.8f, 0.8f, 0.8f);

    [SerializeField]
    private bool ignoreInRayTracing = false;

    private Rigidbody body;

    private SimulatedBodyPrediction predictionBody;

    private LineRenderer lineRenderer;
    private TrailRenderer trailRenderer;

    public bool IgnoreInRayTracing { get => ignoreInRayTracing; }

    private void Awake()
    {
        body = GetComponentInChildren<Rigidbody>();
        body.velocity = initialVelocity;

        predictionBody = GetComponentInChildren<SimulatedBodyPrediction>();

        lineRenderer = GetComponentInChildren<LineRenderer>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    private void OnEnable()
    {
        FindObjectOfType<Simulator>().Add(this);
    }

    private void OnDisable()
    {
        FindObjectOfType<Simulator>()?.Remove(this);
    }

    public bool DrawOrbit => drawOrbit;

    public ISimulatedBody PredictionBody { get => GetPredictionBody(); }

    private SimulatedBodyPrediction GetPredictionBody()
    {
        return predictionBody ?? GetComponentInChildren<SimulatedBodyPrediction>();
    }

    public void ResetPredictionBody()
    {
        var thisBody = body != null ? body : GetComponentInChildren<Rigidbody>();
        var velocity = SimulationHasNotStarted() ? initialVelocity : thisBody.velocity;

        GetPredictionBody().Reset(thisBody.mass, thisBody.position, velocity, transform.localScale.x / 2.0f);
    }

    private bool SimulationHasNotStarted()
    {
        return Simulator.IsPredictingMode()
            && !Simulator.SimulationHasStarted();
    }

    public void ClearOrbit()
    {
        SetOrbitPoints(Enumerable.Empty<Vector3>());
    }

    public void SetOrbitPoints(IEnumerable<Vector3> points, float startColorMultiplier = 1.0f, float endColorMultiplier = 1.0f, float withMultiplier = 1.0f)
    {
        var color = GetColor();
        color.a = 0.6f;

        var lineRenderer = GetLineRenderer();
        lineRenderer.enabled = true;
        lineRenderer.positionCount = points.Count();
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.startColor = color * startColorMultiplier;
        lineRenderer.endColor = color * endColorMultiplier;
        lineRenderer.widthMultiplier = withMultiplier;
    }

    private TrailRenderer GetTrailRenderer()
    {
        return trailRenderer != null ? trailRenderer : GetComponentInChildren<TrailRenderer>();
    }

    public RayTracingSphere GetVisualSphere()
    {
        var color = GetColor();

        var sphere = new RayTracingSphere();
        sphere.albedo = new Vector3(color.r, color.g, color.b);
        sphere.specular = new Vector3(specularColor.r, specularColor.g, specularColor.b);
        sphere.radius = transform.localScale.x / 2.0f;
        sphere.position = transform.position;

        return sphere;
    }

    private Color GetColor()
    {
        return GetTrailRenderer().startColor;
    }

    private LineRenderer GetLineRenderer()
    {
        return lineRenderer != null ? lineRenderer : GetComponentInChildren<LineRenderer>();
    }

    float ISimulatedBody.Mass => body.mass;

    Vector3 ISimulatedBody.Position => body.position;

    Vector3 ISimulatedBody.Velocity => body.velocity;

    Vector3 ISimulatedBody.RelativePosition => throw new System.NotImplementedException();

    void ISimulatedBody.AddForce(Vector3 force)
    {
        body.AddForce(force, ForceMode.Force);
    }

    void ISimulatedBody.CheckCollision(ISimulatedBody other)
    {
        throw new System.NotImplementedException();
    }

    void ISimulatedBody.Simulate(float deltaTime)
    {
        throw new System.NotImplementedException();
    }
}
