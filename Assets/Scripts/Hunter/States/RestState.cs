using UnityEngine;
public class RestState : State
{
    private FSM_Hunter _hunter;

    public RestState(FSM_Hunter hunter, StateMachine stateMachine)
        : base(stateMachine)
    {
        _hunter = hunter;
    }

    public override void Enter()
    {
        _hunter.ChangeColor(Color.yellow);
        _hunter.UpdateStateIcon(FarmerState.Rest);
        base.Enter();

    }

    public override void Update()
    {
        _hunter.RecoverEnergy();

        if (_hunter.IsEnergyFull)
        {
            stateMachine.ChangeState(FarmerState.Patrol);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
