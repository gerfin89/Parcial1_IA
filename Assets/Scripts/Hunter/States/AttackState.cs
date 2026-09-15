using System.Collections.Generic;
using Unity.VisualScripting;
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
        Debug.Log("ENTRANDO A ATTACK");
        _data.target = _hunter.BoidInVision(_data.visionRadius);
        base.Enter();
    }
       

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        if (_data.target == null) return;

        float distance = Vector3.Distance(_hunter.transform.position, _data.target.transform.position);

        if (distance > _data.visionRadius)
        {
            stateMachine.ChangeState(FarmerState.Patrol);
            return;
        }
        if (distance <= _data.meleeAtackRadius)
        {
            
            PerformAttack(true);
        }
        else if (distance <= _data.rangeAttackRadius)
        {
            
            PerformAttack(false);
        }
        else
        {
            
            ChaseHunter();
        }

        

            
    }

    private void ChaseHunter()
    {
        Vector3 targetPosition = new Vector3(_data.target.position.x, _hunter.transform.position.y,_data.target.position.z);
        Vector3 direction = (_data.target.position - _hunter.transform.position).normalized;
        //Debug.Log("ANTES: " + _hunter.transform.position);
        _hunter.transform.position += direction * _hunter.MaxSpeed * Time.deltaTime;
        //Debug.Log("DESPUÉS: " + _hunter.transform.position);
        _hunter.transform.forward = direction;
    }

    [System.Serializable]
    public class AttackData
    {
        public Transform target;
        public float meleeAtackRadius;
        public float rangeAttackRadius;
        public float visionRadius;

    }

    private void PerformAttack(bool isMelee)
    {
        BoidHealth health = _data.target.GetComponent<BoidHealth>();
        if (health != null)
        {
            if (isMelee) 
                health.TakeDamage(1);
            else
                health.ForceDown();
        }


        _hunter.ResetTba();
        stateMachine.ChangeState(FarmerState.Patrol);
    }

    
}
