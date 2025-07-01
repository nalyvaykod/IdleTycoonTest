using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    [Header("UI Elements for Resources")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public Slider xpSlider;

    [Header("UI Elements for Inventory")]
    public TextMeshProUGUI inventoryCapacityText;

    [Header("Menu Buttons")]
    public Button buildButton;
    public Button inventoryButton;
    public Button craftingButton;

    void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerResources != null)
        {
            GameManager.Instance.playerResources.OnMoneyChanged += UpdateMoneyUI;
            GameManager.Instance.playerResources.OnLevelChanged += UpdateLevelUI;
            GameManager.Instance.playerResources.OnXPChanged += UpdateXPUI;
            GameManager.Instance.playerResources.OnXPProgressChanged += UpdateXPBarUI;
        }
        if (GameManager.Instance != null && GameManager.Instance.inventoryManager != null)
        {
            GameManager.Instance.inventoryManager.OnCapacityChanged += UpdateInventoryCapacityUI;
            GameManager.Instance.inventoryManager.OnInventoryChanged += UpdateInventoryCountUI;
        }

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.playerResources != null)
            {
                UpdateMoneyUI(GameManager.Instance.playerResources.Money);
                UpdateLevelUI(GameManager.Instance.playerResources.CurrentLevel);
                UpdateXPUI(GameManager.Instance.playerResources.XP);
                UpdateXPBarUI(GameManager.Instance.playerResources.XP, GameManager.Instance.playerResources.GetXPForCurrentLevelUp());
            }
            if (GameManager.Instance.inventoryManager != null)
            {
                UpdateInventoryCapacityUI(GameManager.Instance.inventoryManager.MaxCapacity);
                UpdateInventoryCountUI();
            }
        }

        //if (buildButton != null) buildButton.onClick.AddListener(OnBuildMenuButtonClicked);
        if (inventoryButton != null) inventoryButton.onClick.AddListener(OnInventoryMenuButtonClicked);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.playerResources != null)
            {
                GameManager.Instance.playerResources.OnMoneyChanged -= UpdateMoneyUI;
                GameManager.Instance.playerResources.OnLevelChanged -= UpdateLevelUI;
                GameManager.Instance.playerResources.OnXPChanged -= UpdateXPUI;
                GameManager.Instance.playerResources.OnXPProgressChanged -= UpdateXPBarUI;
            }
            if (GameManager.Instance.inventoryManager != null)
            {
                GameManager.Instance.inventoryManager.OnCapacityChanged -= UpdateInventoryCapacityUI;
                GameManager.Instance.inventoryManager.OnInventoryChanged -= UpdateInventoryCountUI;
            }
        }

        //if (buildButton != null) buildButton.onClick.RemoveListener(OnBuildMenuButtonClicked);
        if (inventoryButton != null) inventoryButton.onClick.RemoveListener(OnInventoryMenuButtonClicked);
    }

    private void UpdateMoneyUI(double newMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"Гроші: {newMoney:F2}";
        }
    }

    private void UpdateLevelUI(int newLevel)
    {
        if (levelText != null)
        {
            levelText.text = $"Рівень: {newLevel}";
        }
    }

    private void UpdateXPUI(int newXP)
    {
    }

    private void UpdateXPBarUI(int currentXP, int xpToNextLevel)
    {
        if (xpSlider != null && xpText != null)
        {
            if (xpToNextLevel == -1)
            {
                xpSlider.value = xpSlider.maxValue;
                xpText.text = $"MAX Рівень ({currentXP} XP)";
            }
            else
            {
                xpSlider.maxValue = xpToNextLevel;
                xpSlider.value = currentXP;
                xpText.text = $"{currentXP}/{xpToNextLevel} XP";
            }
        }
    }

    private void UpdateInventoryCapacityUI(int newCapacity)
    {
        if (inventoryCapacityText != null && GameManager.Instance != null && GameManager.Instance.inventoryManager != null)
        {
            inventoryCapacityText.text = $"Місткість інвентарю: {GameManager.Instance.inventoryManager.CurrentItemCount}/{newCapacity}";
        }
    }

    private void UpdateInventoryCountUI()
    {
        if (inventoryCapacityText != null && GameManager.Instance != null && GameManager.Instance.inventoryManager != null)
        {
            inventoryCapacityText.text = $"Місткість інвентарю: {GameManager.Instance.inventoryManager.CurrentItemCount}/{GameManager.Instance.inventoryManager.MaxCapacity}";
        }
    }

    public void OnInventoryMenuButtonClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleInventoryMenu();
        }
    }

}
