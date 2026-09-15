using System.IO;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;

public class BoidSteering : Agent
{
    [SerializeField] private Transform _target;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private Agent _targetAgent;
    [SerializeField] private float detectionRadius;
    

    
    [SerializeField] private float separationRadius;
    [SerializeField] private float alignmentRadius;
    [SerializeField] private float cohesionRadius;

    [SerializeField, Range(0f, 3f)] private float separationWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float alignmentWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float cohesionWeight = 1f;

    private BoidHealth _health;

    public enum steeringModes { Seek, Flee, Arrive, Evade, Pursuit, Flocking }
    public steeringModes currentSteering;

    private void Start()
    {
        BoidManager.instance.RegisterBoid(this);
        _health = GetComponent<BoidHealth>();
        //allAgents.Add(this);
        Vector3 randomDirection = new Vector3(Random.Range(-1, 1), 0f, Random.Range(1, -1));
        _velocity += randomDirection.normalized * maxSpeed;

    }

    void Update()
    {
        //BoidHealth health = GetComponent<BoidHealth>();
        if (_health != null && _health.IsDown) 
        {
            _velocity=Vector3.zero;
            return;
        }
        if (IsDetection())
        {
            currentSteering = steeringModes.Evade;
        }
        else
        {
            currentSteering = steeringModes.Flocking;
        }
        Vector3 steering = SteeringVector();
        Debug.Log("Steering calculado: " + steering + " | Velocity: " + _velocity + " | Boids en manager: " + BoidManager.instance.allBoids.Count);


        _velocity += SteeringVector();
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity != Vector3.zero)
            transform.forward = _velocity;
        transform.position = GoatPen.instance.OutOfGoatPen(transform.position);
    }

    private Vector3 SteeringVector()
    {
        switch (currentSteering)
        {
            case steeringModes.Seek:
                return Seek(_target.position);
            case steeringModes.Flee:
                return Flee(_target.position);
            case steeringModes.Arrive:
                return Arrive(_target.position);
            case steeringModes.Evade:
                return Evade(_targetAgent);
            case steeringModes.Pursuit:
                return Pursuit(_targetAgent);
            case steeringModes.Flocking:
                return Flocking();
            default:
                return Vector3.zero;

        }

    }

    private bool IsDetection()
    {
        if (_targetAgent == null) return false;
        return Vector3.Distance(transform.position, _targetAgent.transform.position) <= detectionRadius;
    }

    private Vector3 Flocking()
    {
        return CalculateSeparation(BoidManager.instance.allBoids, separationRadius) * separationWeight
                + CalculateAlignment(BoidManager.instance.allBoids, alignmentRadius) * alignmentWeight
                + CalculateCohesion(BoidManager.instance.allBoids, cohesionRadius) * cohesionWeight;
    }

    private Vector3 CalculateSeparation(List<BoidSteering> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (var item in list)
        {
            if (item == this) continue;

            if (Vector3.Distance(item.transform.position, transform.position) <= radius)
            {
                desired += (item.transform.position - transform.position);
                count++;
            }

        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return CalculateSteering(-desired.normalized * maxSpeed);
    }

    private Vector3 CalculateAlignment(List<BoidSteering> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (var item in list)
        {
            if (item == this) continue;

            if (Vector3.Distance(item.transform.position, transform.position) <= radius)
            {
                desired += item.Velocity;
                count++;
            }

        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return CalculateSteering(desired.normalized * maxSpeed);
    }
    private Vector3 CalculateCohesion(List<BoidSteering> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (var item in list)
        {
            if (item == this) continue;

            if (Vector3.Distance(item.transform.position, transform.position) <= radius)
            {
                desired += item.transform.position;
                count++;
            }

        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return Seek(desired);
    }

    private Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;

        steering = Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);
        return steering;

    }
    private Vector3 DesiredVector(Vector3 target)
    {
        Vector3 desired = (target - transform.position).normalized;
        desired *= maxSpeed;
        return desired;
    }

    private Vector3 Seek(Vector3 target)
    {
        var desired = DesiredVector(target);

        return CalculateSteering(desired);
    }

    private Vector3 Flee(Vector3 target)
    {
        var desired = DesiredVector(target);

        return CalculateSteering(-desired);
    }

    private Vector3 Arrive(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;

        if (distance < minDistance)
        {
            return CalculateSteering(Vector3.zero);

        }

        float targetSpeed = maxSpeed * (distance / slowingDistance);
        float desiredSpeed = Mathf.Min(targetSpeed, maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        return CalculateSteering(desired);
    }

    private Vector3 CalculateFuture(Agent target)
    {
        Vector3 direccion = target.transform.position - transform.position;

        float distance = direccion.magnitude;

        float prediction = distance / (maxSpeed + target.Velocity.magnitude);

        Vector3 futurePosition = target.transform.position + target.Velocity * prediction;
        return (futurePosition);
    }

    private Vector3 Pursuit(Agent target)
    {
        var futurePosition = CalculateFuture(target);
        return Seek(futurePosition);
    }

    private Vector3 Evade(Agent target)
    {
        var futurePosition = CalculateFuture(target);
        return Flee(futurePosition);
    }

    private void OnDestroy()
    {
        BoidManager.instance.UnregisterBoid(this);
    }
}