using UnityEngine;

public sealed class SimulatedBodyPrediction : MonoBehaviour, ISimulatedBody
{
    private float mass = 0.0f;
    private Vector3 position = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 forceAccumulator = Vector3.zero;

    private float colliderRadius;
    private bool hasCollided = false;

    [SerializeField]
    private SimulatedBody referenceBody;

    public void Reset(float mass, Vector3 position, Vector3 velocity, float colliderRadius)
    {
        this.mass = mass;
        this.position = position;
        this.velocity = velocity;
        this.colliderRadius = colliderRadius;

        hasCollided = false;
    }

    float ISimulatedBody.Mass => mass;

    Vector3 ISimulatedBody.Position => position;

    Vector3 ISimulatedBody.Velocity => velocity;

    Vector3 ISimulatedBody.RelativePosition
    {
        get
        {
            return CalculatePositionRelativeToReferenceBody();
        }
    }

    private Vector3 CalculatePositionRelativeToReferenceBody()
    {
        if (referenceBody == null || !referenceBody.isActiveAndEnabled)
        {
            return position;
        }

        var referenceBodyPredictedPoint = referenceBody.PredictionBody.Position;

        return referenceBody.GetComponent<Rigidbody>().position + position - referenceBodyPredictedPoint;
    }

    void ISimulatedBody.AddForce(Vector3 force)
    {
        forceAccumulator += force;
    }

    void ISimulatedBody.Simulate(float deltaTime)
    {
        if (!hasCollided)
        {
            var acceleration = forceAccumulator / mass;

            velocity += acceleration * deltaTime;
            position += velocity * deltaTime;
        }

        forceAccumulator = Vector3.zero;
    }

    void ISimulatedBody.CheckCollision(ISimulatedBody other)
    {
        var otherBody = (SimulatedBodyPrediction)other;

        if (Intersects(position, colliderRadius, otherBody.position, otherBody.colliderRadius))
        {
            CollideWith(otherBody);
        }
    }

    private void CollideWith(SimulatedBodyPrediction otherBody)
    {
        // TODO: do something here?
        hasCollided = otherBody.hasCollided = true;
    }

    private static bool Intersects(Vector3 aCenter, float aRadius, Vector3 bCenter, float bRadius)
    {
        return Vector3.Distance(aCenter, bCenter) <= aRadius + bRadius;
    }
}