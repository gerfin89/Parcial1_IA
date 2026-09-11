using System.IO;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Steering : Agent
    {
    [SerializeField] private Transform _target;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private Agent _targetAgent;

    public enum steeringModes { Seek, Flee, Arrive, Evade, Pursuit}
    public steeringModes currentSteering;
    
    void Update()
    {
        _velocity += SteeringVector();
        //_velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
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
                return Arrive(_target.position);
            case steeringModes.Evade:
                return Evade(_targetAgent);
            case steeringModes.Pursuit:
                return Pursuit(_targetAgent);
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

    private Vector3 Arrive(Vector3 target)
    {
        Vector3 direction = target - transform.position;
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
        return Seek (futurePosition);
   }

    private Vector3 Evade (Agent target)
    {
        var futurePosition = CalculateFuture(target);
        return Flee (futurePosition);
    }
     
}
