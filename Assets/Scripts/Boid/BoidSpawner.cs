using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private List<Transform> spawnPoints;

    public void SpawnBoid()
    {
        
        Transform spawnPoint = spawnPoints [(Random.Range (0, spawnPoints.Count))];
        Instantiate (boidPrefab, spawnPoint.position,spawnPoint.rotation);
        
    }

}
