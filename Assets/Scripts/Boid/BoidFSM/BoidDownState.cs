using UnityEngine;

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


