using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq; 

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
            Debug.Log($"Save path: {savePath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        GameData data = new GameData();

        if (GameManager.Instance.playerResources != null)
        {
            data.money = GameManager.Instance.playerResources.Money;
            data.level = GameManager.Instance.playerResources.CurrentLevel;
            data.xp = GameManager.Instance.playerResources.XP;
        }
        else
        {
            Debug.LogWarning("PlayerResources not found.");
        }

        if (GameManager.Instance.inventoryManager != null)
        {
            data.inventoryItems = new Dictionary<string, int>(GameManager.Instance.inventoryManager.items);
        }
        else
        {
            Debug.LogWarning("InventoryManager not found.");
        }

        data.equipmentData = new List<EquipmentSaveData>();
        UpgradeableEquipment[] allEquipment = FindObjectsOfType<UpgradeableEquipment>();
        foreach (var equipment in allEquipment)
        {
            data.equipmentData.Add(new EquipmentSaveData
            {
                equipmentId = equipment.equipmentId,
                currentLevel = equipment.CurrentLevel
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved!");
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (GameManager.Instance.playerResources != null)
            {
                GameManager.Instance.playerResources.Money = data.money;
                GameManager.Instance.playerResources.CurrentLevel = data.level;
                GameManager.Instance.playerResources.XP = data.xp;
            }
            else
            {
                Debug.LogWarning("PlayerResources not found!.");
            }

            if (GameManager.Instance.inventoryManager != null)
            {
                GameManager.Instance.inventoryManager.SetInventory(data.inventoryItems);
            }
            else
            {
                Debug.LogWarning("InventoryManager not found.");
            }

            if (data.equipmentData != null)
            {
                foreach (var savedEquipment in data.equipmentData)
                {
                    UpgradeableEquipment equipment = FindObjectsOfType<UpgradeableEquipment>()
                                                        .FirstOrDefault(e => e.equipmentId == savedEquipment.equipmentId);
                    if (equipment != null)
                    {
                        equipment.SetLevel(savedEquipment.currentLevel); 
                        Debug.Log($"Loaded level {savedEquipment.currentLevel} for equipment {equipment.name} ({equipment.equipmentId})");
                    }
                    else
                    {
                        Debug.LogWarning($"Equipment with ID {savedEquipment.equipmentId} not found in scene for loading. It might have been removed or its ID changed.");
                    }
                }
            }

            Debug.Log("Game loaded!");
        }
        else
        {
            Debug.Log("Save File not found. Start new game!");
            if (GameManager.Instance.playerResources != null)
            {
                GameManager.Instance.playerResources.Money = 100.0;
                GameManager.Instance.playerResources.CurrentLevel = 1;
                GameManager.Instance.playerResources.XP = 0;
            }
        }
    }
}
