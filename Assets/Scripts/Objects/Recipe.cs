using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "CafeTycoon/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public Item resultItem;
    public int resultQuantity;
    public List<IngredientCost> ingredients;
    public float craftTime;
    public double sellingPrice;
    public int xpReward;

    [TextArea(3, 5)]
    public string description;

    public string GetResultItemId()
    {
        return resultItem != null ? resultItem.id : string.Empty;
    }
}

[System.Serializable]
public class IngredientCost
{
    public Item ingredient;
    public int quantity;
}