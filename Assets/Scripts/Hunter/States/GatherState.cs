using UnityEngine;
using static PatrolState;

public class GatherState : State
{
    public FSM_Hunter _hunter;
    public GatherData _data;
    private float _timer;
    public GatherState(FSM_Hunter hunter, GatherData data, StateMachine stateMachine) : base(stateMachine)
    {
        _hunter = hunter;
        _data = data;
    }
    public override void Enter()
    {
        _timer = 0;
        _data.target = _hunter.FallendBoidInVision(_data.visionRadius);
        _hunter.UpdateStateIcon(FarmerState.Gather);
        _hunter.ChangeColor(Color.cyan);
    }
    public override void Exit() 
    {
        _timer = 0;
        _data.target = null;
    }
    public override void Update() 
    {
        if (!HasValidTarget()) return;

        float distance = Vector3.Distance(_hunter.transform.position, _data.target.position);

        if (distance > _data.gatherDistance)
        {
            MoveTowardsTarget();
        }
        else
        {
            ProcessGathering();
        }
    }
    private bool HasValidTarget()
    {
        if (_data.target == null)
        {
            stateMachine.ChangeState(FarmerState.Patrol);
            return false;
        }

        BoidHealth health = _data.target.GetComponent<BoidHealth>();

        if (health == null || !health.IsDown)
        {
            stateMachine.ChangeState(FarmerState.Patrol);
            return false;
        }

        return true;
    }
    private void MoveTowardsTarget()
    {
        Vector3 steering = _hunter.Movement.Seek(_data.target.position,_hunter.Velocity,_hunter.MaxSpeed);

        _hunter.Movement.Move(_hunter.Velocity,steering,_hunter.MaxSpeed);
    }
    private void ProcessGathering()
    {
        _timer += Time.deltaTime;

        if (_timer < _data.gatherTime)
            return;

        BoidSteering boid = _data.target.GetComponent<BoidSteering>();

        if (boid != null)
        {
            boid.SetBoidSpawner(_data.boidSpawner);
            boid.StartRespawn();
        }
        _data.target = null;
        stateMachine.ChangeState(FarmerState.Patrol);
    }
    [System.Serializable]
    public class GatherData
    {
        public Transform target;
        public float visionRadius;
        public float gatherDistance;
        public float gatherTime;
        public BoidSpawner boidSpawner;
    }
}

    