using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class GoatPen : MonoBehaviour
{
    public static GoatPen instance { get; private set; }
    [SerializeField] private float height;
    [SerializeField] private float width;
    [SerializeField] private bool drawGizmos;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
    }

    public Vector3 OutOfGoatPen(Vector3 position)
    {
        Vector3 newPosition = position;

        if(position.x > width/2) newPosition.x = -width/2;
        if (position.x < -width / 2) newPosition.x = width / 2;
        if (position.z > height / 2) newPosition.z = -height / 2;
        if (position.z < -height / 2) newPosition.z = height / 2;

        return newPosition;
    }
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, 0, height));
    }
}


