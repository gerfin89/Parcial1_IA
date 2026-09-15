using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
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
    [SerializeField] private float tba;
    private StateMachine stateMachine;

    private float _tbaTimer;
    public bool isTbaReady =>_tbaTimer <= 0;
    public void ResetTba() => _tbaTimer = tba;
    

    private void Awake()
    {
        stateMachine = new StateMachine();

        IdleState idleState = new IdleState(stateMachine);
        PatrolState patrolState = new PatrolState(this, dataPatrol,dataAttack.visionRadius, stateMachine);
        AttackState attackState = new AttackState(this, dataAttack, stateMachine);
       // GatherState gatherState = new GatherState();

        stateMachine.RegisterState(FarmerState.Idle,idleState);
        stateMachine.RegisterState(FarmerState.Patrol, patrolState);
        stateMachine.RegisterState(FarmerState.Attack, attackState);

        stateMachine.ChangeState(FarmerState.Patrol);
        //stateMachine.ChangeState(FarmerState.Attack);
       // stateMachine.ChangeState(gatherState);

    }


    private void Update()
    {
        if(_tbaTimer > 0f)
        {
            _tbaTimer -= Time.deltaTime;
        }
        stateMachine.Update();
    }

    public Transform BoidInVision(float visionRadius)
    {
        Transform inVision = null;

        float minDistance = Mathf.Infinity;

        foreach (var Boid in BoidManager.instance.allBoids)
        {
            float dist = Vector3.Distance(transform.position, Boid.transform.position);

            if (dist <= visionRadius && dist < minDistance)
            {
                minDistance = dist;

                inVision = Boid.transform;
            }
        }
        return inVision;
    }


}
