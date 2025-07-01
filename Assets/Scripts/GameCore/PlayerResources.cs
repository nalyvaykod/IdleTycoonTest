using UnityEngine;
using System; 

public class PlayerResources : MonoBehaviour
{
    public static PlayerResources Instance { get; private set; }

    [Header("Player Resources")]
    [SerializeField] private double money = 0.0; 
    [SerializeField] private int currentLevel = 1; 
    [SerializeField] private int xp = 0; 

    [Header("Progression Settings")]
    [SerializeField] private int[] xpToNextLevel = { 100, 250, 500, 1000, 2000 }; 

    
    public event Action<double> OnMoneyChanged; 
    public event Action<int> OnLevelChanged; 
    public event Action<int> OnXPChanged;
    public event Action<int, int> OnXPProgressChanged; 

    public double Money
    {
        get { return money; }
        set
        {
            money = value;
            OnMoneyChanged?.Invoke(money); 
        }
    }

    public int CurrentLevel
    {
        get { return currentLevel; }
        set
        {
            currentLevel = value;
            OnLevelChanged?.Invoke(currentLevel); 
            UpdateXPProgress(); 
        }
    }

    public int XP
    {
        get { return xp; }
        set
        {
            xp = value;
            OnXPChanged?.Invoke(xp); 
            CheckForLevelUp(); 
            UpdateXPProgress(); 
        }
    }

    public int GetXPForCurrentLevelUp()
    {
        if (currentLevel - 1 >= 0 && currentLevel - 1 < xpToNextLevel.Length)
        {
            return xpToNextLevel[currentLevel - 1];
        }
        return -1;
    }

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
        OnMoneyChanged?.Invoke(money);
        OnLevelChanged?.Invoke(currentLevel);
        OnXPChanged?.Invoke(xp);
        UpdateXPProgress(); 
    }

    public void AddMoney(double amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempted to add a negative amount of money.");
            return;
        }
        Money += amount; 
        Debug.Log($"Added {amount:F2} money. Current balance: {Money:F2}");
    }

    public bool SpendMoney(double amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempted to spend a negative amount of money.");
            return false;
        }
        if (Money >= amount)
        {
            Money -= amount; 
            Debug.Log($"Spent {amount:F2} money. Current balance: {Money:F2}");
            return true;
        }
        else
        {
            Debug.Log($"Insufficient funds. Needed {amount:F2}, have {Money:F2}.");
            return false;
        }
    }

    public void AddXP(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempted to add a negative amount of XP.");
            return;
        }
        XP += amount; 
        Debug.Log($"Added {amount} XP. Current XP: {XP}");
    }

    private void CheckForLevelUp()
    {
        if (CurrentLevel <= xpToNextLevel.Length)
        {
            int requiredXPForNextLevel = GetXPForCurrentLevelUp();

            if (requiredXPForNextLevel != -1 && XP >= requiredXPForNextLevel)
            {
                XP -= requiredXPForNextLevel; 
                CurrentLevel++; 
                Debug.Log($"Level Up! Reached Level {CurrentLevel}!");
            }
        }
        else
        {
            Debug.Log("Maximum level reached or XP To Next Level array is not configured for further levels.");
        }
    }

    private void UpdateXPProgress()
    {
        int xpNeeded = GetXPForCurrentLevelUp();
        if (xpNeeded != -1)
        {
            OnXPProgressChanged?.Invoke(xp, xpNeeded); 
        }
        else
        {
            OnXPProgressChanged?.Invoke(xp, -1); 
        }
    }
}
