using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
public enum BoidStateType
{   Floking, 
    Evade,
    Arrive,
    Down
}

public class BoidFlokingState : State
{

    private readonly BoidSteering _boid;

    public BoidFlokingState(StateMachine stateMachine, BoidSteering boid) : base(stateMachine)
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

public class BoidEvadeState : State 
{
    private readonly BoidSteering _boid;

    public BoidEvadeState(StateMachine stateMachine, BoidSteering Boid) : base(stateMachine)
    {
        _boid = Boid;
    }

    public override void Update()
    {
        if  (_boid.IsDown) { stateMachine.ChangeState(BoidStateType.Down); return; }
        if (!_boid.IsThreatDetected(true)) { stateMachine.ChangeState(BoidStateType.Floking); return; }

        _boid.Move(_boid.EvadeSteering());
    }
}  
    

public class BoidArriveState : State 
{
    private readonly BoidSteering _boid;
    private Transform _bait;

    public BoidArriveState(StateMachine stateMachine, BoidSteering boid) : base (stateMachine)
    {
        _boid = boid;
    }

    public override void Enter()
    {
       _boid.TryGetBait(out _bait);
    }

    public override void Update()
    {
        if (_boid.IsDown){ stateMachine.ChangeState(BoidStateType.Down); return; }
        if (_boid.IsThreatDetected(false)) { stateMachine.ChangeState(BoidStateType.Evade); return;}

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

public class BoidDownState : State
{
    private readonly BoidSteering _boid;

    public BoidDownState(StateMachine stateMachine, BoidSteering boid) : base(stateMachine)
    {
        _boid = boid;
    }

    public override void Enter()
    {
        _boid.Sleep();
    }
}
    



