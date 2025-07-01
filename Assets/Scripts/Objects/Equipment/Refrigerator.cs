using UnityEngine;

public class Refrigerator : UpgradeableEquipment
{
    [Header("Refrigerator Settings")]
    [SerializeField] private int baseCapacity = 50;

    protected override void ApplyUpgradeEffects()
    {
        int totalCapacity = baseCapacity;
        for (int i = 0; i < CurrentLevel; i++)
        {
            if (i < upgradeData.upgradeLevels.Count)
            {
                totalCapacity += upgradeData.upgradeLevels[i].capacityIncrease;
            }
        }

        if (GameManager.Instance != null && GameManager.Instance.inventoryManager != null)
        {
            GameManager.Instance.inventoryManager.UpdateMaxCapacity(totalCapacity);
        }
        else
        {
            Debug.LogError("Refrigerator: InventoryManager not found to update capacity! Ensure GameManager and InventoryManager exist in scene.");
        }
        Debug.Log($"Refrigerator {equipmentId} at level {CurrentLevel}. Total Inventory Capacity: {totalCapacity}");
    }

    public void OnUpgradeButtonClicked()
    {
        Upgrade();
    }
}