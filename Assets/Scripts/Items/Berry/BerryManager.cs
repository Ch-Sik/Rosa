using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryManager : MonoBehaviour
{
    private static BerryManager instance;
    public static BerryManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public int berry;

    public UIBerry UIBerry;

    [Button]
    public void AddBerry(int amount)
    {
        berry += amount;

        UIBerry.AddBerryEvent(amount);
    }

    [Button]
    public bool RemoveBerry(int amount)
    {
        if (berry - amount < 0)
            return false;

        berry -= amount;

        UIBerry.AddBerryEvent(-1 * amount);
        return true;
    }
}
