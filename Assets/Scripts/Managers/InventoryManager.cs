using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public Dictionary<string, int> items = new Dictionary<string, int>();

    [Header("Inventory Settings")]
    [SerializeField] private int baseMaxCapacity = 100;
    private int currentMaxCapacity;

    public int MaxCapacity
    {
        get { return currentMaxCapacity; }
        private set
        {
            currentMaxCapacity = value;
            OnCapacityChanged?.Invoke(currentMaxCapacity);
        }
    }

    public int CurrentItemCount { get; private set; } = 0;

    public event Action OnInventoryChanged;
    public event Action<int> OnCapacityChanged;

    void Awake()
    {
        Instance = this;
        MaxCapacity = baseMaxCapacity;
    }

    public void AddItem(Item item, int amount)
    {
        if (item == null)
        {
            Debug.LogError("Attempted to add NULL item to inventory!");
            return;
        }

        if (amount <= 0) return;

        if (CurrentItemCount + amount > MaxCapacity)
        {
            Debug.LogWarning($"Inventory full! Cannot add {amount} x {item.itemName}. Current count: {CurrentItemCount}/{MaxCapacity}");
            return;
        }

        if (items.ContainsKey(item.id))
        {
            items[item.id] += amount;
        }
        else
        {
            items.Add(item.id, amount);
        }
        CurrentItemCount += amount;
        Debug.Log($"Added {amount} x {item.itemName}. Total: {items[item.id]}. Occupied: {CurrentItemCount}/{MaxCapacity}");
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(Item item, int amount)
    {
        if (item == null)
        {
            Debug.LogError("Attempted to remove NULL item from inventory!");
            return false;
        }

        if (amount <= 0) return false;

        if (items.ContainsKey(item.id))
        {
            if (items[item.id] >= amount)
            {
                items[item.id] -= amount;
                CurrentItemCount -= amount;
                if (items[item.id] == 0)
                {
                    items.Remove(item.id);
                }
                Debug.Log($"Removed {amount} x {item.itemName}. Remaining: " + (items.ContainsKey(item.id) ? items[item.id].ToString() : "0") + $". Occupied: {CurrentItemCount}/{MaxCapacity}");
                OnInventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning($"Not enough {item.itemName} to remove. Needed {amount}, have {items[item.id]}");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"Item {item.itemName} not found in inventory.");
            return false;
        }
    }

    public bool HasItem(Item item, int amount)
    {
        if (item == null) return false;
        if (amount <= 0) return true;

        return items.ContainsKey(item.id) && items[item.id] >= amount;
    }

    public int GetItemCount(Item item)
    {
        if (item == null) return 0;
        return items.ContainsKey(item.id) ? items[item.id] : 0;
    }

    public void SetInventory(Dictionary<string, int> savedItems)
    {
        items = savedItems ?? new Dictionary<string, int>();
        CurrentItemCount = 0;
        foreach (var pair in items)
        {
            CurrentItemCount += pair.Value;
        }
        OnInventoryChanged?.Invoke();
        OnCapacityChanged?.Invoke(MaxCapacity);
        Debug.Log("Inventory loaded.");
    }

    public void UpdateMaxCapacity(int newCapacity)
    {
        MaxCapacity = newCapacity;
        Debug.Log($"Max inventory capacity updated to: {MaxCapacity}");
    }
}
