using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [SerializeField] private InventoryManager inventoryManager;

    public event Action<Recipe, float> OnCraftingProgress;
    public event Action<Recipe, Item> OnCraftingComplete;

    private Dictionary<string, Coroutine> activeCrafts = new Dictionary<string, Coroutine>();

    void Awake()
    {
            Instance = this;
    }

    void Start()
    {
        if (inventoryManager == null)
        {
            inventoryManager = GameManager.Instance.inventoryManager;
            if (inventoryManager == null)
            {
                Debug.LogError("CraftingManager: InventoryManager not found!");
            }
        }
    }

    public bool StartCrafting(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogError("Attempting to craft with NULL recipe.");
            return false;
        }

        if (activeCrafts.ContainsKey(recipe.name))
        {
            Debug.LogWarning($"Recipe '{recipe.name}' is already being crafted.");
            return false;
        }

        if (!CanCraft(recipe))
        {
            Debug.LogWarning($"Not enough ingredients to craft '{recipe.name}'.");
            return false;
        }

        foreach (var ingredientCost in recipe.ingredients)
        {
            if (!inventoryManager.RemoveItem(ingredientCost.ingredient, ingredientCost.quantity))
            {
                Debug.LogError($"Error removing ingredient {ingredientCost.ingredient.itemName} for recipe {recipe.name}. Crafting cancelled.");
                return false;
            }
        }

        Debug.Log($"Starting craft: {recipe.recipeName}...");
        Coroutine craftCoroutine = StartCoroutine(DoCrafting(recipe));
        activeCrafts.Add(recipe.name, craftCoroutine);
        return true;
    }

    public bool CanCraft(Recipe recipe)
    {
        if (recipe == null || inventoryManager == null) return false;

        foreach (var ingredientCost in recipe.ingredients)
        {
            if (!inventoryManager.HasItem(ingredientCost.ingredient, ingredientCost.quantity))
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator DoCrafting(Recipe recipe)
    {
        float timer = 0f;
        while (timer < recipe.craftTime)
        {
            timer += Time.deltaTime;
            OnCraftingProgress?.Invoke(recipe, timer / recipe.craftTime);
            yield return null;
        }

        inventoryManager.AddItem(recipe.resultItem, recipe.resultQuantity);
        OnCraftingComplete?.Invoke(recipe, recipe.resultItem);
        Debug.Log($"Crafting '{recipe.recipeName}' completed. Added {recipe.resultQuantity} x {recipe.resultItem.itemName} to inventory.");

        activeCrafts.Remove(recipe.name);
    }

    public float GetCraftingProgress(Recipe recipe)
    {
        if (activeCrafts.ContainsKey(recipe.name))
        {
            return 0f;
        }
        return 0f;
    }
}
