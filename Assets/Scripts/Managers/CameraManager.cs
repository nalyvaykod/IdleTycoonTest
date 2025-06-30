using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [SerializeField] private float moveSpeed = 10.0f; 
    [SerializeField] private float zoomSpeed = 1500.0f; 

    [Header("Camera Height Limits")]
    [SerializeField] private float minZoomY = 10.0f; 
    [SerializeField] private float maxZoomY = 21.0f; 

    [Header("Camera Movement Boundaries (X and Z)")]
    [SerializeField] private float minX = -10.0f; 
    [SerializeField] private float maxX = 10.0f;  
    [SerializeField] private float minZ = -15.0f; 
    [SerializeField] private float maxZ = 1.0f;   

    void Update()
    {
        HandleMovementInput();
        HandleZoomInput();
        ClampCameraPosition();
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxis("Horizontal"); 
        float moveZ = Input.GetAxis("Vertical"); 

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized; 

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            Vector3 newPosition = transform.position;
            newPosition.y -= scroll * zoomSpeed * Time.deltaTime; 

            newPosition.y = Mathf.Clamp(newPosition.y, minZoomY, maxZoomY);
            transform.position = newPosition;
        }
    }

    private void ClampCameraPosition()
    {
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);
        currentPosition.z = Mathf.Clamp(currentPosition.z, minZ, maxZ);
        transform.position = currentPosition;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) / 2f, transform.position.y, (minZ + maxZ) / 2f);
        Vector3 size = new Vector3(maxX - minX, 0.1f, maxZ - minZ); 
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.yellow;
        Vector3 minHeightPos = new Vector3(transform.position.x, minZoomY, transform.position.z);
        Vector3 maxHeightPos = new Vector3(transform.position.x, maxZoomY, transform.position.z);
        Gizmos.DrawLine(minHeightPos, maxHeightPos);
        Gizmos.DrawSphere(minHeightPos, 0.5f); 
        Gizmos.DrawSphere(maxHeightPos, 0.5f); 
    }
}