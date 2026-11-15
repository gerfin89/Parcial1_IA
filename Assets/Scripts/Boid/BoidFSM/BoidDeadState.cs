public class BoidDeadState : State
{
    private readonly BoidSteering _boid;

    public BoidDeadState(StateMachine stateMachine, BoidSteering boid): base(stateMachine)
    {
        _boid = boid;
    }

    public override void Enter()
    {
        _boid.UpdateStateText("Dead");
        _boid.Sleep();
    }
    public override void Update()
    {
        if (_boid.IsRespawning)
        {
            stateMachine.ChangeState(BoidStateType.Respawn);
        }
    }

}