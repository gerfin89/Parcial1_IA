using System.IO;
using UnityEngine;

public class BoidSteering : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;

    [SerializeField] private float minDistance;
    private Vector3 _velocity; //currentVelocity

    
    void Update()
    {
        //Evade();
        //Seek();
        //Flee();
        Arrive();
        
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
    }

   
    private void Seek()
    {
        Vector3 desired = (target.position - transform.position).normalized;
        desired *= maxSpeed;
        Vector3 steering = desired - _velocity;

        steering = Vector3.ClampMagnitude(steering, maxSteering*Time.deltaTime);

        _velocity += steering;
    }

    private void Flee()
    {
        Vector3 desired = (transform.position - target.position).normalized;
        desired *= maxSpeed;
        Vector3 steering = desired - _velocity;

        steering = Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);

        _velocity += steering;
    }

    private void Arrive()
    {
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance < minDistance) 
        {
            _velocity = Vector3.zero;
            return;
        }

        float targetSpeed = maxSpeed *(distance/slowingDistance);
        float desiredSpeed = Mathf.Min(targetSpeed,maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        Vector3 steering = desired - _velocity;

        steering = Vector3.ClampMagnitude(steering,maxSteering*Time.deltaTime);
        _velocity += steering;
    }
}
