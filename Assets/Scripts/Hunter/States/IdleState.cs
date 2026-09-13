using System.Threading;
using UnityEngine;

public class IdleState : State
{
    float _timeToChangePatrol;
    float _timer;

    public IdleState (StateMachine stateMachine) : base(stateMachine)
    {

    }
    public override void Enter()
    {
        _timer = 0;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        
        if(_timeToChangePatrol <= _timer)
        {
            stateMachine.ChangeState(FarmerState.Patrol);
        }
    }
}
