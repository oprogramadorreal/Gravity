using UnityEngine;

// Based on: https://youtu.be/YuPEmRXtwIg
[RequireComponent(typeof(ParticleSystem))]
public sealed class StarField : MonoBehaviour
{
    private ParticleSystem.Particle[] stars;
    private ParticleSystem system;

    [SerializeField]
    private Color starColor = Color.white;

    [SerializeField]
    private int numberOfStars = 600;

    [SerializeField]
    private float starSize = 0.35f;

    [SerializeField]
    private float maxStarDistance = 60.0f;

    [SerializeField]
    private float minStarDistance = 15.0f;

    private void Start()
    {
        system = GetComponent<ParticleSystem>();
        CreateStars();
    }

    private void CreateStars()
    {
        stars = new ParticleSystem.Particle[numberOfStars];

        for (var i = 0; i < numberOfStars; ++i)
        {
            stars[i].position = transform.position + Random.insideUnitSphere * maxStarDistance;
            stars[i].startColor = new Color(starColor.r, starColor.g, starColor.b, starColor.a);
            stars[i].startSize = starSize;
        }
    }

    private void Update()
    {
        var hasChanged = false;

        var maxStarDistanceSqr = maxStarDistance * maxStarDistance;
        var minStarDistanceSqr = minStarDistance * minStarDistance;

        for (var i = 0; i < numberOfStars; ++i)
        {
            if (CalculateStarDistanceSqr(i) > maxStarDistanceSqr)
            {
                stars[i].position = transform.position + Random.insideUnitSphere.normalized * maxStarDistance;
                hasChanged = true;
            }

            var starDistanceSqr = CalculateStarDistanceSqr(i);

            if (starDistanceSqr <= minStarDistanceSqr)
            {
                var percentage = starDistanceSqr / minStarDistanceSqr;
                hasChanged = FadeOutStar(i, percentage);
            }
        }

        if (hasChanged)
        {
            system.SetParticles(stars, stars.Length);
        }
    }

    private bool FadeOutStar(int starIndex, float fadeValue)
    {
        var newStarSize = fadeValue * starSize;

        if (Mathf.Approximately(stars[starIndex].startSize, newStarSize))
        {
            return false;
        }

        var currentColor = stars[starIndex].startColor;
        stars[starIndex].startColor = new Color(currentColor.r, currentColor.g, currentColor.b, fadeValue);
        stars[starIndex].startSize = newStarSize;

        return true;
    }

    private float CalculateStarDistanceSqr(int starIndex)
    {
        return (stars[starIndex].position - transform.position).sqrMagnitude;
    }
}
