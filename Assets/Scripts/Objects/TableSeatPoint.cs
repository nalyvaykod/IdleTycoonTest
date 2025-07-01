using UnityEngine;

public class TableSeatPoint : MonoBehaviour
{
    public bool IsOccupied { get; set; } = false;

    void OnDrawGizmos()
    {
        Gizmos.color = IsOccupied ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}