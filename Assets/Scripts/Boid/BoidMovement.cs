using UnityEngine;

public class BoidMovement : MonoBehaviour
{
    public void UpdateRotation(Vector3 velocity)
    {
        if (velocity != Vector3.zero)
            transform.forward = velocity;
    }
    public Vector3 UpdateMovement( Vector3 velocity,Vector3 steering,float maxSpeed)
    {
        velocity += steering;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;
        return velocity;
    }
    public bool CheckBoundaries(GoatPen goatPen)
    {
        if (goatPen == null)
            return false;

        bool isOutside = goatPen.IsOutside(transform.position);
        transform.position = goatPen.OutOfGoatPen(transform.position);
        return isOutside;
    }
    public Vector3 ResetMovement(float maxSpeed)
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f),0f,Random.Range(-1f, 1f));
        return randomDirection.normalized * maxSpeed;
    }
    public Vector3 StopMovement()
    {
        return Vector3.zero;
    }
    public Vector3 CalculateSteering(Vector3 desired,Vector3 velocity,float maxSteering)
    {
        Vector3 steering = desired - velocity;
        steering = Vector3.ClampMagnitude(steering,maxSteering * Time.deltaTime);
        return steering;
    }
    public Vector3 DesiredVector(Vector3 target, float maxSpeed)
    {
        Vector3 desired = (target - transform.position).normalized;
        desired *= maxSpeed;
        return desired;
    }
    public Vector3 Seek(Vector3 target,Vector3 velocity,float maxSpeed,float maxSteering)
    {
        Vector3 desired = DesiredVector(target, maxSpeed);
        return CalculateSteering(desired,velocity,maxSteering);
    }
    public Vector3 Flee(Vector3 target,Vector3 velocity,float maxSpeed,float maxSteering)
    {
        Vector3 desired = DesiredVector(target, maxSpeed);
        return CalculateSteering(-desired,velocity,maxSteering);
    }
    public Vector3 Arrive(Vector3 target,Vector3 velocity,float maxSpeed,float maxSteering,float minDistance,float slowingDistance)
    {
        Vector3 direction = target - transform.position;

        float distance = direction.magnitude;
        if (distance < minDistance)
            return Vector3.zero;

        float targetSpeed = maxSpeed * (distance / slowingDistance);
        float desiredSpeed = Mathf.Min(targetSpeed, maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        return CalculateSteering(desired,velocity,maxSteering);
    }
    public Vector3 CalculateFuture(Agent target, float maxSpeed)
    {
        Vector3 direccion = target.transform.position - transform.position;

        float distance = direccion.magnitude;
        float prediction = distance / (maxSpeed + target.Velocity.magnitude);

        Vector3 futurePosition = target.transform.position + target.Velocity * prediction;
        return futurePosition;
    }
    public Vector3 Evade(Agent target,Vector3 velocity,float maxSpeed,float maxSteering)
    {
        Vector3 futurePosition = CalculateFuture(target, maxSpeed);

        return Flee(futurePosition,velocity,maxSpeed,maxSteering);
    }
}