using System.Collections.Generic;
using UnityEngine;
using static PatrolState;

public class AttackState : State
{
    public FSM_Hunter _hunter;
    public AttackData _data;




    public AttackState(FSM_Hunter hunter, AttackData data, StateMachine stateMachine) : base(stateMachine)
    {
        _hunter = hunter;
        _data = data;

    }

    public override void Enter()
    {
        base.Enter();
    }
       

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {



        stateMachine.ChangeState(FarmerState.Patrol);
    }

    [System.Serializable]
    public class AttackData
    {
        public float meleeAtackRadius;
        public float rangeAttackRadius;
        public float visionRadius;

    }
}
