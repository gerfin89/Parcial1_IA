using System.IO;
using UnityEngine;

public class BoidSteering : MonoBehaviour
{
    [SerializeField] private Transform hunter;
    [SerializeField] private float maxSpeed;

    private Vector3 _velocity; //currentVelocity

    
    void Update()
    {
        //Evade();
        Seek();        
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
    }

   
    private void Seek()
    {
        Vector3 desired = (hunter.position - transform.position).normalized;
        desired *= maxSpeed;
        Vector3 steering = desired - _velocity;

        _velocity = steering;
    }
}
