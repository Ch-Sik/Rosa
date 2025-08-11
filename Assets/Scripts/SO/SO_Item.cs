using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Item_0", menuName = "Inventory/Item")]
public class SO_Item : ScriptableObject
{
    public string itemName;
    [FormerlySerializedAs("itemCode")]
    public ItemCode code;
    [FormerlySerializedAs("itemImage")]
    public Sprite sprite;
    public ItemRarity rarity;
    public int maxQuantity = 1;
    [TextArea(0, 10)] public string itemDescription;
}

public enum ItemRarity
{
    normal,
    speical
}