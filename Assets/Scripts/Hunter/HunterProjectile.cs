using UnityEngine;

public class HunterProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;

    private Vector3 direction;

    public void Initialize(Transform target)
    {
        Vector3 targetPosition = target.position + Vector3.up * 0.5f;

        direction = (targetPosition - transform.position).normalized;

        transform.forward = direction;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        BoidHealth health = other.GetComponentInParent<BoidHealth>();

        if (health == null || health.IsDown)
            return;

        health.TakeDamage(1);

        Destroy(gameObject);
    }
}