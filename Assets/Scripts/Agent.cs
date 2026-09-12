using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] protected float maxSpeed;
    protected Vector3 _velocity;

    public Vector3 Velocity => _velocity;
    public float MaxSpeed => maxSpeed;


}
