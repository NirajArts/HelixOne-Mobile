using System.Collections;
using UnityEngine;

public class GlitchingPlatform : MonoBehaviour
{
    public GameObject platform;              // The platform that will glitch
    public Transform[] glitchPoints;         // Array of points to teleport through
    public float[] timeToGlitchOnNext;       // Time array defining how long to stay at each point
    public ParticleSystem glitchParticles;   // Particle system to play during teleportation
    public float scaleSpeed = 1f;            // Speed at which the platform scales down/up
    public bool useRandomTimes = false;      // Toggle to use random times if arrays are unequal

    private int currentIndex = 0;            // Index for tracking current glitch point
    private bool isGlitching = false;        // Bool to prevent multiple glitches at once

    void Start()
    {
        if (glitchPoints.Length > 0 && (timeToGlitchOnNext.Length == glitchPoints.Length || useRandomTimes))
        {
            // Move platform to the first position
            platform.transform.position = glitchPoints[0].position;
            StartCoroutine(StartGlitchCycle());
        }
        else
        {
            Debug.LogError("Points array must be non-zero, and time array can either match or use random times.");
        }
    }

    IEnumerator StartGlitchCycle()
    {
        while (true)
        {
            // Choose time: either corresponding to the current index or random from array
            float timeToStay = useRandomTimes ? timeToGlitchOnNext[Random.Range(0, timeToGlitchOnNext.Length)]
                                              : timeToGlitchOnNext[currentIndex];

            // Wait for the chosen time
            yield return new WaitForSeconds(timeToStay);

            // Start glitch effect
            yield return StartCoroutine(GlitchToNextPoint());

            // Move to the next point in the array, cycle back to the start
            currentIndex = (currentIndex + 1) % glitchPoints.Length;
        }
    }

    IEnumerator GlitchToNextPoint()
    {
        // Start scaling down and playing particles
        isGlitching = true;
        glitchParticles.Play();
        yield return StartCoroutine(ScalePlatform(Vector3.zero, scaleSpeed));

        // Move platform to the next point
        platform.transform.position = glitchPoints[currentIndex].position;

        // Scale up the platform and play particles at the new position
        glitchParticles.Play();
        yield return StartCoroutine(ScalePlatform(Vector3.one, scaleSpeed));

        isGlitching = false;
    }

    IEnumerator ScalePlatform(Vector3 targetScale, float speed)
    {
        Vector3 initialScale = platform.transform.localScale;

        float progress = 0f;
        while (progress < 1f)
        {
            platform.transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
            progress += Time.deltaTime * speed;
            yield return null;
        }

        platform.transform.localScale = targetScale; // Ensure it's set exactly to target scale
    }
}