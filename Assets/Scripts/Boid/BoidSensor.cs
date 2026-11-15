using System.Collections.Generic;
using UnityEngine;

public class BoidSensor : MonoBehaviour
{
    private Agent _targetAgent;

    private readonly List<BoidSteering> _nearbyBoids = new List<BoidSteering>();
    private readonly List<Bait> _nearbyBaits = new List<Bait>();

    public Agent TargetAgent => _targetAgent;
    public List<BoidSteering> NearbyBoids => _nearbyBoids;
    public List<Bait> NearbyBaits => _nearbyBaits;

    private void OnTriggerEnter(Collider other)
    {
        FSM_Hunter hunter = other.GetComponentInParent<FSM_Hunter>();

        if (hunter != null)
        {
            _targetAgent = hunter;
        }
        BoidSteering otherBoid = other.GetComponentInParent<BoidSteering>();

        if (otherBoid != null && otherBoid != GetComponent<BoidSteering>() &&!_nearbyBoids.Contains(otherBoid))
        {
            _nearbyBoids.Add(otherBoid);
        }
        Bait bait = other.GetComponentInParent<Bait>();

        if (bait != null && !_nearbyBaits.Contains(bait))
        {
            _nearbyBaits.Add(bait);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        FSM_Hunter hunter = other.GetComponentInParent<FSM_Hunter>();

        if (hunter != null && _targetAgent == hunter)
        {
            _targetAgent = null;
        }
        BoidSteering otherBoid = other.GetComponentInParent<BoidSteering>();

        if (otherBoid != null && otherBoid != GetComponent<BoidSteering>())
        {
            _nearbyBoids.Remove(otherBoid);
        }
        Bait bait = other.GetComponentInParent<Bait>();

        if (bait != null)
        {
            _nearbyBaits.Remove(bait);
        }
    }
    public bool IsTargetWithinRange(float radius)
    {
        if (_targetAgent == null)
            return false;

        return Vector3.Distance(transform.position, _targetAgent.transform.position) <= radius;
    }
    public void CleanDestroyedBaits()
    {
        _nearbyBaits.RemoveAll(b => b == null);
    }
    public bool TryGetClosestBait(float baitDetectionRadius,float hunterSafetyRadius,out Transform closestBait)
    {
        CleanDestroyedBaits();

        closestBait = null;
        float minDist = Mathf.Infinity;

        foreach (Bait candidate in _nearbyBaits)
        {
            if (_targetAgent != null)
            {
                float baitToHunter = Vector3.Distance(candidate.transform.position,_targetAgent.transform.position);

                if (baitToHunter <= hunterSafetyRadius)
                    continue;
            }

            float distance = Vector3.Distance(transform.position,candidate.transform.position);

            if (distance <= baitDetectionRadius && distance < minDist)
            {
                minDist = distance;
                closestBait = candidate.transform;
            }
        }

        return closestBait != null;
    }
}
