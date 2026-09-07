using System.IO;
using UnityEngine;

public class BoidSteering : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;

    [SerializeField] private float minDistance;
    private Vector3 _velocity; //currentVelocity

    public enum steeringModes { Seek, Flee, Arrive, Evade, Pursuit}
    public steeringModes currentSteering;
    
    void Update()
    {
        _velocity += SteeringVector();
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
        transform.position += _velocity * Time.deltaTime;
        
        if(_velocity != Vector3.zero )
        transform.forward = _velocity;
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
                return Arrive();
            case steeringModes.Evade:
                return Vector3.zero;
            default:
                return Vector3.zero;

        }

    }

    private Vector3 CalculateSteering(Vector3 desired )
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

    private Vector3 Arrive()
    {
        Vector3 direction = _target.position - transform.position;
        float distance = direction.magnitude;

        if (distance < minDistance) 
        {
            return CalculateSteering (Vector3.zero);
            
        }

        float targetSpeed = maxSpeed *(distance/slowingDistance);
        float desiredSpeed = Mathf.Min(targetSpeed,maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        return CalculateSteering(desired);
    }
}
