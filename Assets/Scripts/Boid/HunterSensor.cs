using System.Collections.Generic;
using UnityEngine;

public class HunterSensor : MonoBehaviour
{
    private readonly List<BoidSteering> _nearbyBoids = new List<BoidSteering>();

    public void CleanNearbyBoids()
    {
        _nearbyBoids.RemoveAll(boid => boid == null);
    }

    public Transform BoidInVision(float visionRadius)
    {
        Transform inVision = null;
        float minDistance = Mathf.Infinity;

        foreach (var boid in _nearbyBoids)
        {
            if (boid == null) continue;

            BoidHealth health = boid.Health;
            if (health != null && health.IsDown) continue;

            float dist = Vector3.Distance(transform.position,boid.transform.position);

            if (dist <= visionRadius && dist < minDistance)
            {
                minDistance = dist;
                inVision = boid.transform;
            }
        }

        return inVision;
    }

    public Transform FallendBoidInVision(float visionRadius)
    {
        Transform inVision = null;
        float minDistance = Mathf.Infinity;

        foreach (var boid in _nearbyBoids)
        {
            if (boid == null) continue;

            BoidHealth health = boid.Health;
            if (health == null || !health.IsDown) continue;
            if (boid.IsRespawning) continue;

            float dist = Vector3.Distance(transform.position,boid.transform.position);

            if (dist <= visionRadius && dist < minDistance)
            {
                minDistance = dist;
                inVision = boid.transform;
            }
        }

        return inVision;
    }

    private void OnTriggerEnter(Collider other)
    {
        BoidSteering boid =
            other.GetComponentInParent<BoidSteering>();

        if (boid != null && !_nearbyBoids.Contains(boid))
            _nearbyBoids.Add(boid);
    }

    private void OnTriggerExit(Collider other)
    {
        BoidSteering boid =
            other.GetComponentInParent<BoidSteering>();

        if (boid != null)
            _nearbyBoids.Remove(boid);
    }
}