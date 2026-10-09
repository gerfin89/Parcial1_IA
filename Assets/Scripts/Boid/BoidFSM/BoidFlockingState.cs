using UnityEngine;

public class BoidFlockingState : State
{

    private readonly BoidSteering _boid;

    public BoidFlockingState(StateMachine stateMachine, BoidSteering boid) : base(stateMachine)
    {
        _boid = boid;
    }

    public override void Update()
    {
        if (_boid.IsDown) { stateMachine.ChangeState(BoidStateType.Down); return; }
        if (_boid.IsThreatDetected(false)) { stateMachine.ChangeState(BoidStateType.Evade); return; }
        if (_boid.TryGetBait(out _)) { stateMachine.ChangeState(BoidStateType.Arrive); return; }

        _boid.Move(_boid.Flocking());
    }

}
