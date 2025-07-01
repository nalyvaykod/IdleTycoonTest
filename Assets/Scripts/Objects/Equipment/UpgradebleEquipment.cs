using UnityEngine;
using System;

public abstract class UpgradeableEquipment : MonoBehaviour
{
    [Header("Equipment Settings")]
    public string equipmentId;
    public EquipmentUpgradeData upgradeData;

    [SerializeField] private int currentLevel = 0;

    public int CurrentLevel
    {
        get { return currentLevel; }
        private set
        {
            currentLevel = value;
            OnEquipmentUpgraded?.Invoke(this, currentLevel);
            ApplyUpgradeEffects();
        }
    }

    public event Action<UpgradeableEquipment, int> OnEquipmentUpgraded;

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(equipmentId))
        {
            equipmentId = System.Guid.NewGuid().ToString();
            Debug.LogWarning($"Generated new ID for equipment {gameObject.name}: {equipmentId}. Save scene to persist ID!");
        }
    }

    protected virtual void Start()
    {
        ApplyUpgradeEffects();
    }

    public bool CanUpgrade()
    {
        if (upgradeData == null || CurrentLevel >= upgradeData.upgradeLevels.Count)
        {
            Debug.Log("Max upgrade level reached or upgrade data missing.");
            return false;
        }

        UpgradeLevelData nextLevelData = upgradeData.upgradeLevels[CurrentLevel];
        if (GameManager.Instance.playerResources.Money >= nextLevelData.cost)
        {
            return true;
        }
        else
        {
            Debug.Log($"Not enough money to upgrade {upgradeData.equipmentName}. Needed: {nextLevelData.cost}, Have: {GameManager.Instance.playerResources.Money}");
            return false;
        }
    }

    public void Upgrade()
    {
        if (!CanUpgrade())
        {
            return;
        }

        UpgradeLevelData nextLevelData = upgradeData.upgradeLevels[CurrentLevel];
        if (GameManager.Instance.playerResources.SpendMoney(nextLevelData.cost))
        {
            CurrentLevel++;
            Debug.Log($"Equipment {upgradeData.equipmentName} upgraded to level {CurrentLevel}.");
        }
        else
        {
            Debug.LogError($"Error deducting money for {upgradeData.equipmentName} upgrade.");
        }
    }

    public void SetLevel(int level)
    {
        if (level >= 0 && level <= upgradeData.upgradeLevels.Count)
        {
            currentLevel = level;
            OnEquipmentUpgraded?.Invoke(this, currentLevel);
            ApplyUpgradeEffects();
        }
        else
        {
            Debug.LogError($"Attempted to set invalid level {level} for {equipmentId}.");
        }
    }

    protected abstract void ApplyUpgradeEffects();

    public UpgradeLevelData GetNextUpgradeLevelData()
    {
        if (upgradeData != null && CurrentLevel < upgradeData.upgradeLevels.Count)
        {
            return upgradeData.upgradeLevels[CurrentLevel];
        }
        return null;
    }
}
