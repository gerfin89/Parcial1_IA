using System.Collections.Generic;
using UnityEngine;

public class BaitManager : MonoBehaviour
{
   public static BaitManager instance;

   public List<Bait> allBaits = new List<Bait>();

    private void Awake()
    {
        instance = this;
    }

    public void RegisterBait(Bait bait)
    {
        if (!allBaits.Contains(bait))
        {
            allBaits.Add(bait);
        }
    }

    public void UnRegisterBait(Bait bait)
    {
        allBaits.Remove(bait);
    }
}
