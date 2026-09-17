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
    private HunterFeedBack _feedback;
    private float baitTimer;
    private List<GameObject> spawnedBait = new List<GameObject>(); 
    public PatrolState(FSM_Hunter hunter, PatrolData data, float visionRadius, float gatherVisionRadius, StateMachine stateMachine) : base(stateMachine) 
    {
        _hunter = hunter;
        _data = data;
        _visionRadius = visionRadius;
        _gatherVisionRadius = gatherVisionRadius;
        _feedback = hunter.GetComponentInChildren<HunterFeedBack>();
    }
    public override void Enter()
    {
        base.Enter();
        baitTimer = _data.baitSpawnTime;

        if (_feedback != null)
        {
            _feedback.SetState("PATROL");
        }
        
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

       // Vector3 randomPos = new Vector3(Random.Range(_data.spawnAreaMin.x, _data.spawnAreaMax.x),0,(Random.Range(_data.spawnAreaMin.z,_data.spawnAreaMax.z)));
        Debug.Log("BAIT CREADO - HUNTER POS: " + _hunter.transform.position);
        GameObject bait = Object.Instantiate(_data.baitPrefab,_hunter.transform.position,Quaternion.identity);
        spawnedBait.Add(bait);

        baitTimer = _data.baitSpawnTime;
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

        [Header("Bait")]
        public GameObject baitPrefab;
        public float baitSpawnTime = 5f;
        public int maxBaits = 5;

        public Vector3 spawnAreaMin;
        public Vector3 spawnAreaMax;
    }



}
