using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
   public List<BoidSteering> allBoids = new List<BoidSteering>();
    
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
