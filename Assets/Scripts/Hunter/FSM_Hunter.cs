using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum FarmerState
{
    Idle,
    Patrol,
    Attack,

}

public class FSM_Hunter : Agent
{
    
    [SerializeField] private PatrolState.PatrolData dataPatrol;
    [SerializeField] private AttackState.AttackData dataAttack;

    private StateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine();

        IdleState idleState = new IdleState(stateMachine);
        PatrolState patrolState = new PatrolState(this, dataPatrol, stateMachine);
        AttackState attackState = new AttackState(this, dataAttack, stateMachine);
       // GatherState gatherState = new GatherState();

        stateMachine.RegisterState(FarmerState.Idle,idleState);
        stateMachine.RegisterState(FarmerState.Patrol, patrolState);
        stateMachine.RegisterState(FarmerState.Attack, attackState);

        stateMachine.ChangeState(FarmerState.Idle);
        stateMachine.ChangeState(FarmerState.Attack);
       // stateMachine.ChangeState(gatherState);

    }


    private void Update()
    {
        
        stateMachine.Update();
    }

   
}
