using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BodiesFollower : MonoBehaviour
{
    [SerializeField]
    private Transform bodyToFollow;

    [SerializeField]
    private bool followDirection = false;

    private IList<SimulatedBody> allBodies;

    private void Start()
    {
        allBodies = FindObjectsOfType<SimulatedBody>();
    }

    private void Update()
    {
        var targetPosition = GetTargetPosition();

        if (followDirection)
        {
            transform.LookAt(targetPosition);
        }
        else
        {
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }        
    }

    private Vector3 GetTargetPosition()
    {
        if (bodyToFollow != null)
        {
            return bodyToFollow.position;
        }
        
        return CalculateBodiesCenter();
    }

    private Vector3 CalculateBodiesCenter()
    {
        var center = Vector3.zero;

        foreach (var b in allBodies)
        {
            center += b.transform.position;
        }

        return center / allBodies.Count;
    }
}
