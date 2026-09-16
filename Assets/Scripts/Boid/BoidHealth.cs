using UnityEngine;

public class BoidHealth : MonoBehaviour
{
    [SerializeField] int maxHealth;
    private int _currentHealth;
    private bool _isDown;

    [SerializeField] Renderer _renderer;
    [SerializeField] Color _damagedColor = Color.pink;
    [SerializeField] Color _originalColor;

    public bool IsDown => _isDown;

    private void Awake()
    {
        _currentHealth = maxHealth;
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
        }
        
    }

   
    public void TakeDamage(int amount)
    {
        if (_isDown) return;

        _currentHealth -= amount;

        if(_currentHealth  <= 0)
        {
            Down();
        }
    }

    public void ForceDown()
    {
        if (_isDown) return;
        Down();
    }

    private void Down()
    {
        _isDown = true;
        _currentHealth = 0;
        if( _renderer != null ) 
            _renderer.material.color = _damagedColor;
    }
}
