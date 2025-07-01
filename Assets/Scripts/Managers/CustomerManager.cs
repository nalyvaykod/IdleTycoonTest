using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Customer Spawn Settings")]
    public GameObject customerPrefab;
    public Transform entrancePoint;
    public int maxCustomers = 5;
    [SerializeField] private float customerSpawnInterval = 10f;

    private List<TableSeatPoint> allTableSeats = new List<TableSeatPoint>();
    private List<CustomerAI> activeCustomers = new List<CustomerAI>();
    private WorkerAI worker;

    void Awake()
    {
            Instance = this;
    }

    void Start()
    {
        allTableSeats = FindObjectsOfType<TableSeatPoint>().ToList();
        if (allTableSeats.Count == 0)
        {
            Debug.LogWarning("No TableSeatPoint found in scene!");
        }
        else
        {
            Debug.Log($"Found {allTableSeats.Count} seat points.");
        }

        if (entrancePoint == null)
        {
            GameObject entranceObj = GameObject.FindWithTag("EntrancePoint");
            if (entranceObj != null) entrancePoint = entranceObj.transform;
            else Debug.LogError("EntrancePoint not found in scene! Customers will not spawn.");
        }

        worker = GameManager.Instance.workerAI;
        if (worker == null)
        {
            Debug.LogError("CustomerManager: WorkerAI not found in scene!");
        }

        StartCoroutine(SpawnCustomersRoutine());
    }

    private IEnumerator SpawnCustomersRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(customerSpawnInterval);

            if (activeCustomers.Count < maxCustomers && HasFreeTableSeat())
            {
                SpawnCustomer();
            }
        }
    }

    private void SpawnCustomer()
    {
        if (customerPrefab == null || entrancePoint == null)
        {
            Debug.LogError("Cannot spawn customer: prefab or entrance point not set.");
            return;
        }

        GameObject customerObj = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        CustomerAI customer = customerObj.GetComponent<CustomerAI>();
        if (customer != null)
        {
            activeCustomers.Add(customer);
            customer.Initialize(worker);
            Debug.Log($"Spawning new customer: {customer.name}.");
        }
        else
        {
            Debug.LogError("Customer prefab does not contain a CustomerAI component.");
            Destroy(customerObj);
        }
    }

    public TableSeatPoint GetFreeTableSeat()
    {
        foreach (var seat in allTableSeats)
        {
            if (!seat.IsOccupied)
            {
                seat.IsOccupied = true;
                return seat;
            }
        }
        Debug.Log("No free table seats available.");
        return null;
    }

    public bool HasFreeTableSeat()
    {
        return allTableSeats.Any(seat => !seat.IsOccupied);
    }

    public void CustomerExited()
    {
        activeCustomers.RemoveAll(c => c == null);
        Debug.Log($"Customer exited. Active customers: {activeCustomers.Count}");
    }
}
