using UnityEngine;

public class CuttingBoard : UpgradeableEquipment
{
    public float CurrentSpeedMultiplier { get; private set; } = 1.0f;

    protected override void ApplyUpgradeEffects()
    {
        CurrentSpeedMultiplier = 1.0f;
        for (int i = 0; i < CurrentLevel; i++)
        {
            if (i < upgradeData.upgradeLevels.Count)
            {
                CurrentSpeedMultiplier *= upgradeData.upgradeLevels[i].speedMultiplier;
            }
        }
        Debug.Log($"Cutting Board {equipmentId} at level {CurrentLevel}. Speed Multiplier: {CurrentSpeedMultiplier:F2}");
    }

    public void OnUpgradeButtonClicked()
    {
        Upgrade();
    }
}
