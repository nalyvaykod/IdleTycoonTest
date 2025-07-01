using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Systems")]
    public PlayerResources playerResources;
    public InventoryManager inventoryManager;
    public CraftingManager craftingManager;
    public CustomerManager customerManager;
    public BuildManager buildManager;
    public WorkerAI workerAI;
    public UIManager uiManager;

    [Header("Auto Save Settings")]
    [SerializeField] private float autoSaveInterval = 300f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerResources = FindObjectOfType<PlayerResources>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        craftingManager = FindObjectOfType<CraftingManager>();
        customerManager = FindObjectOfType<CustomerManager>();
        buildManager = FindObjectOfType<BuildManager>();
        workerAI = FindObjectOfType<WorkerAI>();
        uiManager = FindObjectOfType<UIManager>();

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGame();
        }
        else
        {
            Debug.LogError("SaveSystem.Instance not found! Ensure SaveSystem exists in the scene.");
        }

        InvokeRepeating("AutoSave", autoSaveInterval, autoSaveInterval);
        Debug.Log("GameManager initialized. Auto-save started.");

        if (inventoryManager != null)
        {
            Item coffeeBeans = Resources.Load<Item>("Items/CoffeeBeans");
            Item milk = Resources.Load<Item>("Items/Milk");
            Item sugar = Resources.Load<Item>("Items/Sugar");
            Item mug = Resources.Load<Item>("Items/Cup"); 

            if (coffeeBeans != null) inventoryManager.AddItem(coffeeBeans, 100);
            else Debug.LogWarning("CoffeeBeans Item ScriptableObject not found in Resources/Items!");

            if (milk != null) inventoryManager.AddItem(milk, 50);
            else Debug.LogWarning("Milk Item ScriptableObject not found in Resources/Items!");

            if (sugar != null) inventoryManager.AddItem(sugar, 200);
            else Debug.LogWarning("Sugar Item ScriptableObject not found in Resources/Items!");

            if (mug != null) inventoryManager.AddItem(mug, 50); 
            else Debug.LogWarning("Mug Item ScriptableObject not found in Resources/Items!");

            Debug.Log("Initial ingredients added to inventory.");
        }
    }

    void AutoSave()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
            Debug.Log("Game auto-saved!");
        }
    }

    public void AddMoneyAndXP(double moneyAmount, int xpAmount)
    {
        if (playerResources != null)
        {
            playerResources.AddMoney(moneyAmount);
            playerResources.AddXP(xpAmount);
            Debug.Log($"Gained: {moneyAmount} money, {xpAmount} XP.");
        }
    }
}
