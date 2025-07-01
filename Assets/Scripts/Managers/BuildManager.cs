using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("Building Settings")]
    [SerializeField] private LayerMask buildLayerMask;
    [SerializeField] private Material placementMaterialValid;
    [SerializeField] private Material placementMaterialInvalid;
    [SerializeField] private float placementOffset = 0.01f;

    private GameObject currentBuildableGhost;
    private BuildableObjectData selectedBuildableData;

    private List<GameObject> builtObjects = new List<GameObject>();

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

    void Update()
    {
        if (currentBuildableGhost != null)
        {
            UpdateBuildableGhostPosition();
            HandlePlacementInput();
        }
    }

    public void StartBuilding(BuildableObjectData buildableData)
    {
        if (buildableData == null || buildableData.prefab == null)
        {
            Debug.LogError("BuildManager: Invalid buildable data or prefab missing.");
            return;
        }

        selectedBuildableData = buildableData;

        if (currentBuildableGhost != null)
        {
            Destroy(currentBuildableGhost);
        }

        currentBuildableGhost = Instantiate(selectedBuildableData.prefab);
        SetGhostMaterial(currentBuildableGhost, placementMaterialInvalid);
        foreach (Collider col in currentBuildableGhost.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        Debug.Log($"Starting build: {selectedBuildableData.objectName}");
    }

    private void UpdateBuildableGhostPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, buildLayerMask))
        {
            currentBuildableGhost.transform.position = hit.point + Vector3.up * placementOffset;

            bool canPlace = CanPlaceObject(hit.point);
            SetGhostMaterial(currentBuildableGhost, canPlace ? placementMaterialValid : placementMaterialInvalid);
        }
    }

    private void HandlePlacementInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, buildLayerMask))
            {
                if (CanPlaceObject(hit.point))
                {
                    PlaceObject(hit.point + Vector3.up * placementOffset);
                }
                else
                {
                    Debug.LogWarning("Cannot place object here. Location occupied or unsuitable.");
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            CancelBuilding();
        }
    }

    private bool CanPlaceObject(Vector3 position)
    {
        return true;
    }

    private void PlaceObject(Vector3 position)
    {
        if (selectedBuildableData == null) return;

        if (GameManager.Instance.playerResources.SpendMoney(selectedBuildableData.cost))
        {
            GameObject newObject = Instantiate(selectedBuildableData.prefab, position, Quaternion.identity);
            builtObjects.Add(newObject);
            Debug.Log($"Placed: {selectedBuildableData.objectName} for {selectedBuildableData.cost:F2} money.");

            Destroy(currentBuildableGhost);
            currentBuildableGhost = null;
            selectedBuildableData = null;
        }
        else
        {
            Debug.LogWarning("Not enough money to build!");
        }
    }

    public void CancelBuilding()
    {
        if (currentBuildableGhost != null)
        {
            Destroy(currentBuildableGhost);
            currentBuildableGhost = null;
            selectedBuildableData = null;
            Debug.Log("Building cancelled.");
        }
    }

    private void SetGhostMaterial(GameObject ghost, Material material)
    {
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.material = material;
        }
    }

    public List<BuiltObjectData> GetBuiltObjectsData()
    {
        List<BuiltObjectData> data = new List<BuiltObjectData>();
        foreach (var obj in builtObjects)
        {
            data.Add(new BuiltObjectData
            {
                objectId = obj.name.Replace("(Clone)", "").Trim(),
                posX = obj.transform.position.x,
                posY = obj.transform.position.y,
                posZ = obj.transform.position.z,
                rotX = obj.transform.rotation.x,
                rotY = obj.transform.rotation.y,
                rotZ = obj.transform.rotation.z,
                rotW = obj.transform.rotation.w
            });
        }
        return data;
    }

    public void LoadBuiltObjects(List<BuiltObjectData> savedData)
    {
        foreach (var obj in builtObjects)
        {
            Destroy(obj);
        }
        builtObjects.Clear();

        foreach (var data in savedData)
        {
            GameObject prefabToLoad = Resources.Load<GameObject>($"Prefabs/Buildables/{data.objectId}");
            if (prefabToLoad != null)
            {
                Vector3 position = new Vector3(data.posX, data.posY, data.posZ);
                Quaternion rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
                GameObject loadedObject = Instantiate(prefabToLoad, position, rotation);
                builtObjects.Add(loadedObject);
            }
            else
            {
                Debug.LogWarning($"Failed to load prefab for object with ID: {data.objectId}");
            }
        }
        Debug.Log($"Loaded {builtObjects.Count} built objects.");
    }
}

