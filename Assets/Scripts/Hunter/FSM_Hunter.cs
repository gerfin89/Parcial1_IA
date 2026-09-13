using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum FarmerState
{
    Idle,
    Patrol,

}

public class FSM_Hunter : Agent
{
    
    [SerializeField] private PatrolState.PatrolData dataPatrol;

    private StateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine();

        IdleState idleState = new IdleState(stateMachine);
        PatrolState patrolState = new PatrolState(this, dataPatrol, stateMachine);
        //AttackState attackState = new AttackState();
       // GatherState gatherState = new GatherState();

        stateMachine.RegisterState(FarmerState.Idle,idleState);
        stateMachine.RegisterState(FarmerState.Patrol, patrolState);


        stateMachine.ChangeState(FarmerState.Idle);
        //stateMachine.ChangeState(attackState);
       // stateMachine.ChangeState(gatherState);

    }


    private void Update()
    {
        
        stateMachine.Update();
    }

   
}
