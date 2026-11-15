using UnityEngine;
using UnityEngine.UI;

public enum FarmerState
{
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
    [SerializeField] private HunterEnergy hunterEnergy;
    [SerializeField] private HunterSensor hunterSensor;
    [SerializeField] private HunterMovement hunterMovement;

    public HunterMovement Movement => hunterMovement;
    public bool IsEnergyEmpty => hunterEnergy.IsEnergyEmpty;
    public bool IsEnergyFull => hunterEnergy.IsEnergyFull;
    private StateMachine stateMachine;
    private Vector3 _previousPosition;
    private Renderer hunterRenderer;

    private float _tbaTimer;
    public bool IsTbaReady =>_tbaTimer <= 0;
    public void ResetTba() => _tbaTimer = tba;

    public void ConsumeEnergy()
    {
        hunterEnergy.ConsumeEnergy();
    }
    public void RecoverEnergy()
    {
        hunterEnergy.RecoverEnergy();
    }
    public void ChangeColor(Color color)
    {
        if (hunterRenderer != null)
            hunterRenderer.material.color = color;
    }  
    private void Awake()
    {
        InitializeComponents();
        InitializeStateMachine();
        if (hunterMovement == null)
            hunterMovement = GetComponent<HunterMovement>();
    }
    private void InitializeComponents()
    {
        hunterRenderer = GetComponentInChildren<Renderer>();
        _previousPosition = transform.position;

        if (hunterEnergy == null)
            hunterEnergy = GetComponent<HunterEnergy>();

        if (hunterSensor == null)
            hunterSensor = GetComponent<HunterSensor>();
    }
    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine();

        RegisterStates();

        stateMachine.ChangeState(FarmerState.Patrol);
    }
    private void RegisterStates()
    {
       
        PatrolState patrolState = new PatrolState(this,dataPatrol,dataAttack.visionRadius,dataGather.visionRadius,stateMachine);

        AttackState attackState = new AttackState(this, dataAttack, stateMachine);

        GatherState gatherState = new GatherState(this, dataGather, stateMachine);

        RestState restState = new RestState(this, stateMachine);

        stateMachine.RegisterState(FarmerState.Patrol, patrolState);
        stateMachine.RegisterState(FarmerState.Attack, attackState);
        stateMachine.RegisterState(FarmerState.Gather, gatherState);
        stateMachine.RegisterState(FarmerState.Rest, restState);
    }
    private void Update()
    {
        CleanNearbyBoids();
        UpdateAttackCooldown();
        UpdateStateMachine();
        UpdateVelocity();
    }
    private void CleanNearbyBoids()
    {
        hunterSensor.CleanNearbyBoids();
    }
    private void UpdateAttackCooldown()
    {
        if (_tbaTimer > 0f)
            _tbaTimer -= Time.deltaTime;
    }
    private void UpdateStateMachine()
    {
        stateMachine.Update();
    }
    private void UpdateVelocity()
    {
        if (Time.deltaTime > 0f)
        {
            _velocity = (transform.position - _previousPosition)/ Time.deltaTime;
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

        HunterProjectile projectile = Instantiate(projectilePrefab,spawnPosition,Quaternion.identity);

        projectile.Initialize(target);
    }
    public Transform BoidInVision(float visionRadius)
    {
        return hunterSensor.BoidInVision(visionRadius);
    }
    public Transform FallendBoidInVision(float visionRadius)
    {
        return hunterSensor.FallendBoidInVision(visionRadius);
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
