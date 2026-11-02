using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FarmerState
{
    Idle,
    Patrol,
    Attack,
    Gather,
    Rest

}

public class FSM_Hunter : Agent
{
      
    [SerializeField] private PatrolState.PatrolData dataPatrol;
    [SerializeField] private AttackState.AttackData dataAttack;
    [SerializeField] private GatherState.GatherData dataGather;    
    [SerializeField] private float tba;

    [Header("Energy Hunter")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyConsumption = 10f;
    [SerializeField] private float energyRecovery = 20f;

    [Header("UI Stamina")]
    [SerializeField] private Slider staminaBar;

    [Header("Hunter State Icons")]
    [SerializeField] private Image stateIcon;
    [SerializeField] private Sprite patrolIcon;
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite gatherIcon;
    [SerializeField] private Sprite restIcon;

    [Header("Hunter Projectile")]
    [SerializeField] private HunterProjectile projectilePrefab;

    [Header("Límit scenary")]
    [SerializeField] private GoatPen goatPen;

    public GoatPen GoatPen => goatPen;
    private float _currentEnergy;
    public bool IsEnergyEmpty => _currentEnergy <= 0f;
    public bool IsEnergyFull => _currentEnergy >= maxEnergy;
    private StateMachine stateMachine;
    private Vector3 _previousPosition;

    private float _tbaTimer;
    public bool IsTbaReady =>_tbaTimer <= 0;
    public void ResetTba() => _tbaTimer = tba;

    public void ConsumeEnergy()
    {
        _currentEnergy -= energyConsumption * Time.deltaTime;
        _currentEnergy = Mathf.Max(0f, _currentEnergy);

        if (staminaBar != null)
        {
            staminaBar.value = _currentEnergy;
        }
    }
    public void RecoverEnergy()
    {
        _currentEnergy += energyRecovery * Time.deltaTime;
        _currentEnergy = Mathf.Min(_currentEnergy, maxEnergy);

        if (staminaBar != null)
        {
            staminaBar.value = _currentEnergy;
        }
    }
    public void ChangeColor(Color color)
    {
        Renderer renderer = GetComponentInChildren<Renderer>();

        if (renderer != null)
            renderer.material.color = color;
    }

    private List<BoidSteering> _nearbyBoid = new List<BoidSteering>();
    

    private void Awake()
    {
        _previousPosition = transform.position;
        _currentEnergy = maxEnergy;

        if (staminaBar != null)
        {
            staminaBar.minValue = 0f;
            staminaBar.maxValue = maxEnergy;
            staminaBar.value = _currentEnergy;
        }

        stateMachine = new StateMachine();

        IdleState idleState = new IdleState(stateMachine);
        PatrolState patrolState = new PatrolState(this, dataPatrol,dataAttack.visionRadius,dataGather.visionRadius, stateMachine);
        AttackState attackState = new AttackState(this, dataAttack, stateMachine);
        GatherState gatherState = new GatherState(this, dataGather,stateMachine);
        RestState restState = new RestState(this, stateMachine);

        stateMachine.RegisterState(FarmerState.Idle,idleState);
        stateMachine.RegisterState(FarmerState.Patrol, patrolState);
        stateMachine.RegisterState(FarmerState.Attack, attackState);
        stateMachine.RegisterState(FarmerState.Gather, gatherState);
        stateMachine.RegisterState(FarmerState.Rest, restState);
        
        stateMachine.ChangeState(FarmerState.Patrol);

        
    }


    private void Update()
    {
        _nearbyBoid.RemoveAll(b => b == null);

        if (_tbaTimer > 0f)
        {
            _tbaTimer -= Time.deltaTime;
        }

        stateMachine.Update();

        if (Time.deltaTime > 0f)
        {
            _velocity = (transform.position - _previousPosition) / Time.deltaTime;
        }

        _previousPosition = transform.position;
    }

    public void UpdateStateIcon(FarmerState state)
    {
        if (stateIcon == null) return;

        switch (state)
        {
            case FarmerState.Patrol:
                stateIcon.sprite = patrolIcon;
                break;

            case FarmerState.Attack:
                stateIcon.sprite = attackIcon;
                break;

            case FarmerState.Gather:
                stateIcon.sprite = gatherIcon;
                break;

            case FarmerState.Rest:
                stateIcon.sprite = restIcon;
                break;

            default:
                stateIcon.sprite = null;
                break;
        }

        stateIcon.enabled = stateIcon.sprite != null;
    }

    public void ShootProjectile(Transform target)
    {
        if (projectilePrefab == null || target == null)
            return;

        Vector3 spawnPosition = transform.position + Vector3.up * 1.2f;

        HunterProjectile projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        projectile.Initialize(target);
    }

    public Transform BoidInVision(float visionRadius)
    {
        Debug.Log("Hunter: Boids detectados = " + _nearbyBoid.Count);
        Transform inVision = null;

        float minDistance = Mathf.Infinity;

        foreach (var boid in _nearbyBoid)
        {
            
            if ( boid==null ) continue;

            BoidHealth health = boid.GetComponent<BoidHealth>();

            if (health != null && health.IsDown) continue;
            float dist = Vector3.Distance(transform.position, boid.transform.position);

            if (dist <= visionRadius && dist < minDistance)
            {
                minDistance = dist;

                inVision = boid.transform;
            }
        }
        return inVision;
    }

    public Transform FallendBoidInVision(float visionRadius)
    {

        Transform inVision = null;
        float minDistance = Mathf.Infinity;

        foreach (var boid in _nearbyBoid)
        {
            if (boid == null) continue;
            BoidHealth health = boid.GetComponent<BoidHealth>();
            if (health == null || !health.IsDown) continue;
            if (boid.IsRespawning) continue;

            float dist = Vector3.Distance(transform.position, boid.transform.position);

            if (dist <= visionRadius && dist < minDistance)
            {
                minDistance = dist;

                inVision = boid.transform;
            }
            
        }
        return (inVision);


    }

    private void OnTriggerEnter(Collider other)
    {
        BoidSteering boid = other.GetComponentInParent<BoidSteering>();

        if (boid != null && !_nearbyBoid.Contains(boid))
        {
            _nearbyBoid.Add(boid);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BoidSteering boid = other.GetComponentInParent<BoidSteering>();

        if (boid != null)
        {
            _nearbyBoid.Remove(boid);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dataAttack.visionRadius);

        Gizmos.color = new Color (1f,0.5f,0.5f);
        Gizmos.DrawWireSphere(transform.position, dataAttack.rangeAttackRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dataAttack.meleeAtackRadius);

    }
}
