using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "CafeTycoon/Item")]
public class Item : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public bool isStackable;

    [TextArea(3, 5)]
    public string description;

    void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
        }
    }
}

public enum ItemType { Ingredient, Product, Tool, Consumable }