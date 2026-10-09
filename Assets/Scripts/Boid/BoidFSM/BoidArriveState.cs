using UnityEngine;

public class BoidArriveState : State
{
    private readonly BoidSteering _boid;
    private Transform _bait;

    public BoidArriveState(StateMachine stateMachine, BoidSteering boid) : base(stateMachine)
    {
        _boid = boid;
    }

    public override void Enter()
    {
        _boid.TryGetBait(out _bait);
    }

    public override void Update()
    {

        if (_boid.IsDown) { stateMachine.ChangeState(BoidStateType.Down); return; }
        if (_boid.IsThreatDetected(false)) { stateMachine.ChangeState(BoidStateType.Evade); return; }

        if (_bait == null && !_boid.TryGetBait(out _bait))
        {
            stateMachine.ChangeState(BoidStateType.Floking);
            return;
        }

        _boid.Move(_boid.ArriveSteering(_bait));

        if (_boid.IsAtBait(_bait))
        {
            _boid.EatBait(_bait);
            stateMachine.ChangeState(BoidStateType.Down);


        }
    }
}