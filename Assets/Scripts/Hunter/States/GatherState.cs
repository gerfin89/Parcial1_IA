using System.Collections.Generic;
using UnityEngine;
using static PatrolState;

public class GatherState : State
{
    public FSM_Hunter _hunter;
    public GatherData _data;
    private HunterFeedBack _feedback;

    private float _timer;


    public GatherState(FSM_Hunter hunter, GatherData data, StateMachine stateMachine) : base(stateMachine)
    {
        _hunter = hunter;
        _data = data;

        _feedback = hunter.GetComponentInChildren<HunterFeedBack>();
    }

    public override void Enter()
    {
        _timer = 0;
        if (_feedback != null)
            _feedback.SetState("GATHER");


        _data.target = _hunter.FallendBoidInVision(_data.visionRadius);
    }

    public override void Exit() 
    {
        _timer = 0;
        _data.target = null;
    }

    public override void Update() 
    {
        if (_data.target == null)
        {
            stateMachine.ChangeState(FarmerState.Patrol);
            return;
        }

        BoidHealth health = _data.target.GetComponent<BoidHealth>();

        if(health == null || !health.IsDown)
        {
            stateMachine.ChangeState(FarmerState.Patrol);
            return;          
        }

        float distance = Vector3.Distance(_hunter.transform.position, _data.target.position);

        if (distance > _data.gatherDistance)
        {
            Vector3 direction = _data.target.position - _hunter.transform.position;
            direction.y = 0;

            _hunter.transform.position += direction.normalized * _hunter.MaxSpeed * Time.deltaTime;
        }
        else
        {
            _timer += Time.deltaTime;

            if (_timer >= _data.gatherTime)
            {
                Object.Destroy(_data.target.gameObject);

                _data.target = null;

                if (_data.boidSpawner != null)
                { 
                _data.boidSpawner.SpawnBoid(); 
                }
                stateMachine.ChangeState(FarmerState.Patrol);
            }
        }

       
    }

   

    [System.Serializable]
    public class GatherData
    {
        public Transform target;
        public float visionRadius;
        public float gatherDistance;
        public float gatherTime;
        public BoidSpawner boidSpawner;


    }
}

    