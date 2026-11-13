
using UnityEngine;
using UnityEngine.UI;

public class HunterEnergy : MonoBehaviour
{
    [Header("Energy Hunter")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyConsumption = 10f;
    [SerializeField] private float energyRecovery = 20f;

    [Header("UI Stamina")]
    [SerializeField] private Slider staminaBar;

    private float _currentEnergy;

    public bool IsEnergyEmpty => _currentEnergy <= 0f;
    public bool IsEnergyFull => _currentEnergy >= maxEnergy;

    private void Awake()
    {
        _currentEnergy = maxEnergy;

        if (staminaBar != null)
        {
            staminaBar.minValue = 0f;
            staminaBar.maxValue = maxEnergy;
            staminaBar.value = _currentEnergy;
        }
    }

    public void ConsumeEnergy()
    {
        _currentEnergy -= energyConsumption * Time.deltaTime;
        _currentEnergy = Mathf.Max(0f, _currentEnergy);
        UpdateStaminaBar();
    }

    public void RecoverEnergy()
    {
        _currentEnergy += energyRecovery * Time.deltaTime;
        _currentEnergy = Mathf.Min(_currentEnergy, maxEnergy);
        UpdateStaminaBar();
    }

    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
            staminaBar.value = _currentEnergy;
    }
}
