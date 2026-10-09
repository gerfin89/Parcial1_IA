using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;


public class BoidSteering : Agent
{
    [Header ("Stats")]
    [SerializeField] private Transform _target;
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private Agent _targetAgent;
    [SerializeField] private float detectionRadius;
    [SerializeField] private float baitDetectionRadius = 8f;
   
                
    [SerializeField] private float separationRadius;
    [SerializeField] private float alignmentRadius;
    [SerializeField] private float cohesionRadius;


    [SerializeField, Range(0f, 3f)] private float separationWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float alignmentWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float cohesionWeight = 1f;

    private BoidHealth _health;
    private List<BoidSteering> _nearbyBoids = new List<BoidSteering>();
    private List<Bait> _nearbyBait = new List<Bait>();

    
    
    [SerializeField] private float evadeExitMultiplier = 1.3f;
    [SerializeField] private float evadeSteeringMultiplier = 3f;
    StateMachine _stateMachine;
    public bool IsDown => _health !=null && _health.IsDown;
    private void Start()
    {
               
        _health = GetComponent<BoidHealth>();
        Vector3 randomDirection = new Vector3(Random.Range(-1, 1), 0f, Random.Range(-1, 1));
        _velocity += randomDirection.normalized * maxSpeed;

        _stateMachine = new StateMachine();
        _stateMachine.RegisterState(BoidStateType.Floking, new BoidFlokingState (_stateMachine, this));
        _stateMachine.RegisterState(BoidStateType.Evade, new BoidEvadeState(_stateMachine, this));
        _stateMachine.RegisterState(BoidStateType.Arrive, new BoidArriveState(_stateMachine, this));
        _stateMachine.RegisterState(BoidStateType.Down, new BoidDownState(_stateMachine, this));
        _stateMachine.ChangeState(BoidStateType.Floking);



    }

    void Update()
    {

       _stateMachine.Update();
    }

    

    public bool IsThreatDetected (bool alreadyEvading)
    {
        if(_targetAgent == null) return false;

        float radius = alreadyEvading ? detectionRadius * evadeExitMultiplier : detectionRadius;
        return Vector3.Distance(transform.position, _targetAgent.transform.position) <= radius;
    }

    public bool TryGetBait (out Transform bait)
    {
        _nearbyBait.RemoveAll(b => b == null);
        float minDist = Mathf.Infinity;
        bait = null;

        foreach (Bait candidate in _nearbyBait)
        {
            if (_targetAgent != null)
            {
                float baitToHunter = Vector3.Distance(candidate.transform.position, _targetAgent.transform.position);
                if (baitToHunter <= detectionRadius * evadeExitMultiplier) continue;
            }

            float distance = Vector3.Distance(transform.position, candidate.transform.position);
            if (distance < baitDetectionRadius && distance < minDist)
            {
                minDist = distance;
                bait = candidate.transform;
            }
        }
        return bait;
    }
    public Vector3 FlokingSteering() => FlokingSteering();
    public Vector3 EvadeSteering() => Evade(_targetAgent) * evadeSteeringMultiplier;
    public Vector3 ArriveSteering(Transform bait) => Arrive(bait.position);

    public bool IsAtBait(Transform bait)
    {
        return Vector3.Distance(transform.position, bait.position) <= minDistance;
    }
    public void EatBait(Transform bait)
    {
        Bait component = bait.GetComponent<Bait>();
        if (component != null) component.TakeDamage(100);
       
    }

    public void Sleep()
    {
        _velocity = Vector3.zero;
        if(_health != null) _health.ForceDown();
    }

    public void Move(Vector3 steering)
    {
        _velocity += steering;
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity != Vector3.zero)
            transform.forward = _velocity;

        transform.position = GoatPen.instance.OutOfGoatPen(transform.position);
    }
   
    public Vector3 Flocking()
    {
        _nearbyBoids.RemoveAll(b => b == null);
        return CalculateSeparation(_nearbyBoids, separationRadius) * separationWeight
                + CalculateAlignment(_nearbyBoids, alignmentRadius) * alignmentWeight
                + CalculateCohesion(_nearbyBoids, cohesionRadius) * cohesionWeight;
    }

    private Vector3 CalculateSeparation(List<BoidSteering> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (var item in list)
        {
            if (item == null) continue;
            if (item == this) continue;

            BoidHealth health = item.GetComponent<BoidHealth>();

            if (health != null && health.IsDown) continue;

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
            if (item == null) continue;
            if (item == this) continue;

            BoidHealth health = item.GetComponent<BoidHealth>();

            if (health != null && health.IsDown) continue;

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
            if (item == null) continue;
            if (item == this) continue;
            
            BoidHealth health = item.GetComponent<BoidHealth>();

            if (health != null && health.IsDown) continue;

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
        
            return (Vector3.zero);        

        float targetSpeed = maxSpeed * (distance / slowingDistance);
        float desiredSpeed = Mathf.Min(targetSpeed, maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        Vector3 steering = CalculateSteering(desired);
        return CalculateSteering(steering);
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

    private void OnTriggerEnter (Collider other)
    {
        BoidSteering otherBoid = other.GetComponent<BoidSteering>();
        if (otherBoid != null && otherBoid != this)
        {
            _nearbyBoids.Add(otherBoid);
        }

        Bait bait = other.GetComponent<Bait>();
        if (bait != null)
        {
            _nearbyBait.Add(bait);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BoidSteering otherBoid = other.GetComponent<BoidSteering>();
        if (otherBoid != null)
        {
            _nearbyBoids.Remove(otherBoid);
        }

        Bait bait = other.GetComponent<Bait>();
        if (bait != null)
        {
            _nearbyBait.Remove(bait);
        }
    }

    private Vector3 Evade(Agent target)
    {
        var futurePosition = CalculateFuture(target); 
        return Flee(futurePosition);
    }

    

  


   /* private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);

        Gizmos.color= Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alignmentRadius);

        Gizmos.color= Color.black;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color= Color.white;
        Gizmos.DrawWireSphere(transform.position, baitDetectionRadius);
    }*/


}