using UnityEngine;

public interface ISimulatedBody
{
    float Mass { get; }

    Vector3 Position { get; }

    Vector3 Velocity { get; }

    Vector3 RelativePosition { get; }

    void CheckCollision(ISimulatedBody other);

    void AddForce(Vector3 force);

    void Simulate(float deltaTime);
}
