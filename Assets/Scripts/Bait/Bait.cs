using UnityEngine;

public class Bait : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    private void Start()
    {
        BaitManager.instance.RegisterBait(this);
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;

        if (_currentHealth <= 0) 
        {
            Destroy(gameObject);
        }
    }

    public void Eat()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if(BaitManager.instance != null)
        {
            BaitManager.instance.UnRegisterBait(this);
        }
    }

}
