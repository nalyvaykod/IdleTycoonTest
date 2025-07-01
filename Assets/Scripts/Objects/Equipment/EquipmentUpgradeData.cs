using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEquipmentUpgradeData", menuName = "CafeTycoon/Equipment/Upgrade Data")]
public class EquipmentUpgradeData : ScriptableObject
{
    public string equipmentName;
    public List<UpgradeLevelData> upgradeLevels = new List<UpgradeLevelData>();
}

[System.Serializable]
public class UpgradeLevelData
{
    public int level;
    public double cost;
    public float speedMultiplier = 1.0f;
    public int capacityIncrease = 0;
    [TextArea(3, 5)]
    public string description;
}