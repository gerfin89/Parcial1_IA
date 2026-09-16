using System.Collections.Generic;
using NUnit.Framework;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PatrolState : State
{
    public FSM_Hunter _hunter;
    public PatrolData _data;
    private int currentNode;
    private float _visionRadius;
    private float _gatherVisionRadius;
    public PatrolState(FSM_Hunter hunter, PatrolData data, float visionRadius, float gatherVisionRadius, StateMachine stateMachine) : base(stateMachine) 
    {
        _hunter = hunter;
        _data = data;
        _visionRadius = visionRadius;
        _gatherVisionRadius = gatherVisionRadius;
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
        
        Transform fallenBoid = _hunter.FallendBoidInVision(_gatherVisionRadius);
        //Debug.Log("Boid detectado: " + boidDetected + " | TBA listo: " + _hunter.IsTbaReady);
        ;
        if (fallenBoid != null)
        {
            stateMachine.ChangeState  (FarmerState.Gather);
            return;
        }

        Transform boidDetected = _hunter.BoidInVision( _visionRadius);

        if (boidDetected != null && _hunter.IsTbaReady)
        {
            stateMachine.ChangeState(FarmerState.Attack);
            return;
        }

        PatrolLoop();

    }


    private void PatrolLoop()
    {
        Debug.Log("PatrolLoop ejecutándose");
        var nextWaypoint = _data.wayPoints[currentNode];

        if (Vector3.Distance(nextWaypoint.position, _data.transform.position) <= _data.wayPointCheckDistance)
        {
            currentNode = currentNode + 1 < _data.wayPoints.Count ? currentNode + 1 : 0;
        }

        var dir = nextWaypoint.position - _data.transform.position;

        _data.transform.position += dir.normalized * _hunter.MaxSpeed * Time.deltaTime;
    }
    [System.Serializable]
    public class PatrolData 
    {
        public List<Transform> wayPoints;
        public Transform transform;
        public float wayPointCheckDistance;

    }



}
