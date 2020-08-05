using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Simulator : MonoBehaviour
{
    [SerializeField]
    private float G = 10.0f;

    [SerializeField]
    private bool autoSimulation = false;

    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float timeScale = 1.0f;

    private static bool simulationHasStarted = false;

    private readonly IList<ISimulatedBody> allBodies = new List<ISimulatedBody>();

    public event EventHandler SimulationStarted;

    private void Start()
    {
        SetupPhysicsAndTiming();
    }

    public void Add(ISimulatedBody body)
    {
        allBodies.Add(body);
    }

    public void Remove(ISimulatedBody body)
    {
        allBodies.Remove(body);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            autoSimulation = !autoSimulation;
            SetupPhysicsAndTiming();
        }

        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            timeScale += 0.01f;
            timeScale = Mathf.Clamp(timeScale, 0.0f, 10.0f);
            SetupPhysicsAndTiming();
        }

        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            timeScale -= 0.01f;
            timeScale = Mathf.Clamp(timeScale, 0.0f, 10.0f);
            SetupPhysicsAndTiming();
        }
    }

    private void FixedUpdate()
    {
        if (!IsPredictingMode())
        {
            ApplyGravityForces(allBodies);
        }
    }

    public void Simulate(IList<ISimulatedBody> bodies)
    {
        ForEachPair(bodies, (a, b) => a.CheckCollision(b));

        ApplyGravityForces(bodies);

        foreach (var p in bodies)
        {
            p.Simulate(Time.fixedDeltaTime);
        }
    }

    private void ApplyGravityForces(IList<ISimulatedBody> bodies)
    {
        ForEachPair(bodies, (a, b) => ApplyGravityForces(a, b));
    }

    private void ForEachPair(IList<ISimulatedBody> bodies, Action<ISimulatedBody, ISimulatedBody> action)
    {
        for (var i = 0; i < bodies.Count; ++i)
        {
            for (var j = i + 1; j < bodies.Count; ++j)
            {
                action(bodies[i], bodies[j]);
            }
        }
    }

    private void ApplyGravityForces(ISimulatedBody bodyA, ISimulatedBody bodyB)
    {
        var forceDirection = bodyB.Position - bodyA.Position;
        var distance2 = Vector3.SqrMagnitude(forceDirection);

        if (!Mathf.Approximately(distance2, 0.0f))
        {
            forceDirection /= Mathf.Sqrt(distance2); // normalize forceDirection

            var F = G * (bodyA.Mass * bodyB.Mass) / distance2;

            bodyA.AddForce(F * forceDirection);
            bodyB.AddForce(-F * forceDirection);
        }
    }

    private void OnValidate()
    {
        SetupPhysicsAndTiming();
    }

    private void SetupPhysicsAndTiming()
    {
        Physics.autoSimulation = autoSimulation;

        Time.timeScale = timeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        if (autoSimulation && Application.isPlaying)
        {
            simulationHasStarted = true;
            SimulationStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    public static bool IsPredictingMode()
    {
        return !Application.isPlaying
            || !Physics.autoSimulation;
    }

    public static bool SimulationHasStarted()
    {
        if (simulationHasStarted && !Application.isPlaying)
        {
            simulationHasStarted = false;
        }

        return simulationHasStarted;
    }
}
