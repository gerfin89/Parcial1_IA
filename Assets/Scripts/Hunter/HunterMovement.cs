using UnityEngine;

public class HunterMovement : MonoBehaviour
{
    [SerializeField] private float maxSteering = 5f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public Vector3 Seek(Vector3 target, Vector3 velocity, float maxSpeed)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 desired = direction.normalized * maxSpeed;
        Vector3 steering = desired - velocity;

        return Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);
    }
    public Vector3 Move(Vector3 velocity, Vector3 steering, float maxSpeed)
    {
        velocity += steering;
        velocity.y = 0f;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.0001f)
            transform.forward = velocity.normalized;

        return velocity;
    }
    public Vector3 Pursuit(Agent target, Vector3 velocity, float maxSpeed)
    {
        if (target == null)
            return Vector3.zero;

        Vector3 targetPosition = target.transform.position;

        float distance = Vector3.Distance(transform.position, targetPosition);
        float predictionTime = distance / Mathf.Max(0.1f, maxSpeed + target.Velocity.magnitude);

        Vector3 futurePosition = targetPosition + target.Velocity * predictionTime;
        return Seek(futurePosition, velocity, maxSpeed);
    }
}