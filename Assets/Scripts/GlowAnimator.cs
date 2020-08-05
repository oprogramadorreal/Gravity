using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public sealed class GlowAnimator : MonoBehaviour
{
    private Material material;

    private readonly System.Random rand = new System.Random();

    private void Start()
    {
        material = GetComponentInChildren<MeshRenderer>().material;
    }

    private void Update()
    {
        var glowIntensity = 5.0f + (float)Math.Cos(Time.unscaledTime * 3.0f) / 2.0f;
        material.SetColor("_EmissionColor", new Color(1.0f, 1.0f, 0.0f) * glowIntensity);
    }
}
