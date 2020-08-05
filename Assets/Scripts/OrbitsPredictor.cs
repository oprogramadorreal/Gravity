using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public sealed class OrbitsPredictor : MonoBehaviour
{
    [SerializeField]
    private Simulator simulator;

    [SerializeField]
    [Range(0, 10000)]
    private int numberOfSteps = 1000;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float orbitAlpha = 0.3f;

    [SerializeField]
    private float orbitWidth = 0.2f;

    private void Awake()
    {
        simulator.SimulationStarted += (o, e) => ClearOrbits();
    }

    private void ClearOrbits()
    {
        foreach (var body in FindObjectsOfType<SimulatedBody>())
        {
            body.ClearOrbit();
        }
    }

    private void DrawOrbits()
    {
        var bodies = FindObjectsOfType<SimulatedBody>()
            .Where(b => b.isActiveAndEnabled);

        var orbitPoints = PredictOrbits(bodies);

        SetOrbitsPoints(bodies, orbitPoints);
    }

    private IEnumerable<IEnumerable<Vector3>> PredictOrbits(IEnumerable<SimulatedBody> bodies)
    {
        var points = new List<LinkedList<Vector3>>(bodies.Count());

        foreach (var body in bodies)
        {
            body.ResetPredictionBody(); // reset previous predictions
            points.Add(new LinkedList<Vector3>());
        }

        var predictionBodies = bodies.Select(b => b.PredictionBody).ToList();

        for (var i = 0; i < numberOfSteps; ++i)
        {
            simulator.Simulate(predictionBodies);

            foreach (var (body, bodyPoints) in predictionBodies.Zip(points, (body, bodyPoints) => (body, bodyPoints)))
            {
                if (CanAddOrbitPointInto(body.Position, bodyPoints))
                {
                    bodyPoints.AddLast(body.RelativePosition);
                }
            }
        }

        return points;
    }

    public static bool CanAddOrbitPointInto(Vector3 point, LinkedList<Vector3> allPoints)
    {
        return allPoints.Count == 0
            || Vector3.Distance(allPoints.Last(), point) > 0.1f;
    }

    private void SetOrbitsPoints(IEnumerable<SimulatedBody> bodies, IEnumerable<IEnumerable<Vector3>> points)
    {
        var bodyAndPoints = bodies.Zip(points, (body, bodyPoints) => (body, bodyPoints));

        foreach (var (body, bodyPoints) in bodyAndPoints)
        {
            if (body.DrawOrbit)
            {
                body.SetOrbitPoints(bodyPoints, 1.0f, orbitAlpha, orbitWidth);
            }
            else
            {
                body.ClearOrbit();
            }
        }
    }

    private void Update()
    {
        if (Simulator.IsPredictingMode())
        {
            DrawOrbits();
        }
    }
}
