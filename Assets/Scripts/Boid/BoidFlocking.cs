using System.Collections.Generic;
using UnityEngine;

public class BoidFlocking : MonoBehaviour
{
    [Header("Flocking - Ratios")]
    [SerializeField] private float separationRadius;
    [SerializeField] private float alignmentRadius;
    [SerializeField] private float cohesionRadius;

    [Header("Flocking")]
    [SerializeField, Range(0f, 3f)] private float separationWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float alignmentWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float cohesionWeight = 1f;

    private BoidSteering _boid;
    private BoidSensor _sensor;

    private void Awake()
    {
        _boid = GetComponent<BoidSteering>();
        _sensor = GetComponent<BoidSensor>();
    }
    private Vector3 CalculateSeparation(List<BoidSteering> neighbors, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (BoidSteering neighbor in neighbors)
        {
            if (neighbor == null || neighbor == _boid)
                continue;

            BoidHealth health = neighbor.Health;

            if (health != null && health.IsDown)
                continue;

            if ((neighbor.transform.position - transform.position).sqrMagnitude <= radius * radius)
            {
                desired += neighbor.transform.position - transform.position;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        desired /= count;

        return _boid.CalculateSteering(-desired.normalized * _boid.MaxSpeed);
    }
    private Vector3 CalculateAlignment(List<BoidSteering> neighbors, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (BoidSteering neighbor in neighbors)
        {
            if (neighbor == null || neighbor == _boid)
                continue;

            BoidHealth health = neighbor.Health;

            if (health != null && health.IsDown)
                continue;

            if ((neighbor.transform.position - transform.position).sqrMagnitude <= radius * radius)
            {
                desired += neighbor.Velocity;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        desired /= count;

        return _boid.CalculateSteering(desired.normalized * _boid.MaxSpeed);
    }
    private Vector3 CalculateCohesion(List<BoidSteering> neighbors, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (BoidSteering neighbor in neighbors)
        {
            if (neighbor == null || neighbor == _boid)
                continue;

            BoidHealth health = neighbor.Health;

            if (health != null && health.IsDown)
                continue;

            if ((neighbor.transform.position - transform.position).sqrMagnitude <= radius * radius)
            {
                desired += neighbor.transform.position;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        desired /= count;

        return _boid.SeekForFlocking(desired);
    }
    public Vector3 CalculateFlocking()
    {
        Vector3 separation = CalculateSeparation(_sensor.NearbyBoids, separationRadius);

        Vector3 alignment = CalculateAlignment(_sensor.NearbyBoids, alignmentRadius);

        Vector3 cohesion = CalculateCohesion(_sensor.NearbyBoids, cohesionRadius);

        return separation * separationWeight + alignment * alignmentWeight + cohesion * cohesionWeight;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alignmentRadius);
    }
}
