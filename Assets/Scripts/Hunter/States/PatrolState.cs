using System.Collections.Generic;
using UnityEngine;

public class PatrolState : State
{
    public FSM_Hunter _hunter;
    public PatrolData _data;
    private int currentNode;
    private float _visionRadius;
    private float _gatherVisionRadius;
    private float baitTimer;
    private List<GameObject> spawnedBait = new List<GameObject>(); 
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
        _hunter.ChangeColor(Color.blue);
        _hunter.UpdateStateIcon(FarmerState.Patrol);
        baitTimer = _data.baitSpawnTime;
                       
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        
        Transform fallenBoid = _hunter.FallendBoidInVision(_gatherVisionRadius);
        
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

        SpawnBait();

        PatrolLoop();

    }

    private void SpawnBait()
    {
        baitTimer -= Time.deltaTime;

        if (baitTimer > 0)
            return;

        spawnedBait.RemoveAll(x => x == null);

        if(spawnedBait.Count >= _data.maxBaits)
        {
            baitTimer = _data.baitSpawnTime;
            return; 
        }
              
        GameObject bait = Object.Instantiate(_data.baitPrefab,_hunter.transform.position,Quaternion.identity);
        spawnedBait.Add(bait);

        baitTimer = _data.baitSpawnTime;
    }


    private void PatrolLoop()
    {
        var nextWaypoint = _data.wayPoints[currentNode];

        if (Vector3.Distance(nextWaypoint.position, _data.transform.position) <= _data.wayPointCheckDistance)
        {
            currentNode = currentNode + 1 < _data.wayPoints.Count ? currentNode + 1 : 0;
            nextWaypoint = _data.wayPoints[currentNode];
        }

        var dir = nextWaypoint.position - _data.transform.position;

        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);

            _data.transform.rotation = Quaternion.Slerp(_data.transform.rotation,targetRotation,8f * Time.deltaTime);
        }

        _data.transform.position += dir.normalized * _hunter.MaxSpeed * Time.deltaTime;
    }

    [System.Serializable]
    public class PatrolData 
    {
        public List<Transform> wayPoints;
        public Transform transform;
        public float wayPointCheckDistance;

        [Header("Bait")]
        public GameObject baitPrefab;
        public float baitSpawnTime = 5f;
        public int maxBaits = 5;

        public Vector3 spawnAreaMin;
        public Vector3 spawnAreaMax;
    }



}
