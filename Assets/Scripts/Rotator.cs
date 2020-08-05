using UnityEngine;

public sealed class Rotator : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float speed = 100.0f;

    void FixedUpdate()
    {
        transform.RotateAround(target.position, Vector3.forward, speed * Time.fixedDeltaTime);
    }
}
