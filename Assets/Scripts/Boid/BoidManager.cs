using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public static BoidManager instance;
    public List<BoidSteering> allBoids = new List<BoidSteering>();
    private void Awake()
    {
        instance = this;
    }

    public void RegisterBoid(BoidSteering boid)
    {
        if (!allBoids.Contains(boid))
        {
            allBoids.Add(boid);
        } 
    }

    public void UnregisterBoid(BoidSteering boid)
    {
        allBoids.Remove(boid);
    }
}
