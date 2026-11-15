using TMPro;
using UnityEngine;

public class BoidVisual : MonoBehaviour
{
    [Header("UI Estado Boid")]
    [SerializeField] private TextMeshPro stateText;

    private Renderer[] _renderers;
    private Canvas[] _canvases;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _canvases = GetComponentsInChildren<Canvas>(true);
    }
    public void SetVisible(bool visible)
    {
        foreach (Renderer renderer in _renderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }

        foreach (Canvas canvas in _canvases)
        {
            if (canvas != null)
                canvas.enabled = visible;
        }
    }
    public void UpdateStateText(string stateName)
    {
        if (stateText != null)
        {
            stateText.text = stateName;
        }
    }
}