using UnityEngine;
using static PatrolState;


public class AttackState : State
{
   
    public FSM_Hunter _hunter;
    public AttackData _data;
    private Vector3 _lastKnownPosition;
    private int _targetBoundaryCrossings;




    public AttackState(FSM_Hunter hunter, AttackData data, StateMachine stateMachine) : base(stateMachine)
    {
        _hunter = hunter;
        _data = data;

       
    }

    public override void Enter()
    {
        base.Enter();
        _data.target = _hunter.BoidInVision(_data.visionRadius);
        if (_data.target != null)
        {
            _lastKnownPosition = _data.target.position;

            BoidSteering boid = _data.target.GetComponent<BoidSteering>();

            if (boid != null)
            {
                _targetBoundaryCrossings = boid.BoundaryCrossings;
            }
        }
        _hunter.UpdateStateIcon(FarmerState.Attack);
        _hunter.ChangeColor(Color.red);        
    }


    public override void Exit()
    {
        _data.target = null;
        base.Exit();
    }

    public override void Update()
    {
        _hunter.ConsumeEnergy();

        if (_hunter.IsEnergyEmpty)
        {
            stateMachine.ChangeState(FarmerState.Rest);
            return;
        }

        if (_data.target == null)
        {
            _data.target = _hunter.BoidInVision(_data.visionRadius);

            if (_data.target == null)
            {
                Vector3 direction = _lastKnownPosition - _hunter.transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.01f)
                {
                    _hunter.transform.position += direction.normalized * Mathf.Min(_hunter.MaxSpeed * Time.deltaTime, direction.magnitude);

                    _hunter.transform.forward = direction.normalized;
                }

                return;
            }

            _lastKnownPosition = _data.target.position;

            BoidSteering newTargetBoid = _data.target.GetComponent<BoidSteering>();

            if (newTargetBoid != null)
            {
                _targetBoundaryCrossings = newTargetBoid.BoundaryCrossings;
            }
        }

        BoidSteering targetBoid = _data.target.GetComponent<BoidSteering>();

        if (targetBoid != null &&
            targetBoid.BoundaryCrossings > _targetBoundaryCrossings)
        {
            stateMachine.ChangeState(FarmerState.Patrol);
            return;
        }

        Transform deadBoid = _hunter.FallendBoidInVision(_data.visionRadius);

        if (deadBoid != null)
        {
            stateMachine.ChangeState(FarmerState.Gather);
            return;
        }

        BoidHealth health = _data.target.GetComponent<BoidHealth>();

        if (health != null && health.IsDown)
        {
            _data.target = _hunter.BoidInVision(_data.visionRadius);

            if (_data.target != null)
            {
                _lastKnownPosition = _data.target.position;

                BoidSteering nextBoid = _data.target.GetComponent<BoidSteering>();

                if (nextBoid != null)
                {
                    _targetBoundaryCrossings = nextBoid.BoundaryCrossings;
                }
            }

            return;
        }

        _lastKnownPosition = _data.target.position;

        float distance = Vector3.Distance(_hunter.transform.position, _data.target.position);

        if (distance <= _data.meleeAtackRadius)
        {
            PerformAttack(true);
        }
        else if (distance <= _data.rangeAttackRadius)
        {
            PerformAttack(false);
        }
        else
        {
            ChaseHunter();
        }
    }

    private void ChaseHunter()
    {


        BoidSteering boid = _data.target.GetComponent<BoidSteering>();

        Vector3 targetPosition = _data.target.position;

        if (boid != null)
        {
            float distance = Vector3.Distance(_hunter.transform.position,targetPosition);

            float predictionTime = distance /
                Mathf.Max(0.1f, _hunter.MaxSpeed + boid.Velocity.magnitude);

            targetPosition += boid.Velocity * predictionTime;
        }

        targetPosition.y = _hunter.transform.position.y;

        Vector3 direction =
            (targetPosition - _hunter.transform.position).normalized;

        _hunter.transform.position += direction * _hunter.MaxSpeed * Time.deltaTime;
        
        _hunter.transform.forward = direction;
    }

    [System.Serializable]
    public class AttackData
    {
        public Transform target;
        public float meleeAtackRadius;
        public float rangeAttackRadius;
        public float visionRadius;

    }

    private void PerformAttack(bool isMelee)
    {
        if (!_hunter.IsTbaReady) return;

        BoidHealth health = _data.target.GetComponent<BoidHealth>();

        if (health != null)
        {
            if (isMelee)
            {
                health.TakeDamage(2);
            }
            else
            {
                _hunter.ShootProjectile(_data.target);
            }
        }

        _hunter.ResetTba();
    }


}
