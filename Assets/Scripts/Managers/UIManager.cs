using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject gameplayCanvas;
    public GameObject inventoryMenuCanvas;

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
        if (gameplayCanvas == null) Debug.LogError("UIManager: Gameplay Canvas not set! Drag the corresponding Canvas in the Inspector.");
        if (inventoryMenuCanvas == null) Debug.LogWarning("UIManager: Inventory Menu Canvas not set!");

        ShowPanel(gameplayCanvas);
        HidePanel(inventoryMenuCanvas);
    }

    public void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    public void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }


    public void ToggleInventoryMenu()
    {
        if (inventoryMenuCanvas != null)
        {
            inventoryMenuCanvas.SetActive(!inventoryMenuCanvas.activeSelf);
        }
    }

    public void ShowGameplayUI()
    {
        ShowPanel(gameplayCanvas);
        HidePanel(inventoryMenuCanvas);
    }
}