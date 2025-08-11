using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataManager
{
    Dictionary<ItemCode, SO_Item> dict;
    public ItemDataManager()
    {
        dict = new Dictionary<ItemCode, SO_Item>();
        SO_Item[] items = Resources.LoadAll<SO_Item>("Items");
        foreach (var item in items)
        {
            Debug.Log($"{item.code} added to item database");
            dict.Add(item.code, item);
        }
    }

    public SO_Item GetItemData(ItemCode code)
    {
        return dict[code];
    }
}
