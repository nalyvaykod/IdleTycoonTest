using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System; 

public class WorkerAI : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Work Positions")]
    public Transform counterOrderPoint;
    public Transform kitchenPrepPoint;
    public Transform workerIdlePoint;

    [Header("Kitchen Equipment")]
    public Stove stove;
    public Oven oven;
    public CuttingBoard cuttingBoard;
    public Refrigerator refrigerator;

    private Queue<Order> orderQueue = new Queue<Order>();
    private Order currentOrder;

    public bool IsTakingOrder { get; private set; } = false;

    private string instanceId; 

    public event Action<string> OnTaskChanged;

    private string _currentWorkerTask = "Idle";
    public string CurrentWorkerTask
    {
        get { return _currentWorkerTask; }
        private set
        {
            if (_currentWorkerTask != value)
            {
                _currentWorkerTask = value;
                OnTaskChanged?.Invoke(_currentWorkerTask);
            }
        }
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("WorkerAI: NavMeshAgent not found on object!");
        }
        agent.speed = 2.0f;
        agent.stoppingDistance = 0.5f;
        instanceId = System.Guid.NewGuid().ToString().Substring(0, 8); 
        Debug.Log($"WorkerAI [{instanceId}]: Awake.");
    }

    void Start()
    {
        counterOrderPoint = GameObject.FindWithTag("CounterOrderPoint")?.transform;
        kitchenPrepPoint = GameObject.FindWithTag("KitchenPrepPoint")?.transform;
        workerIdlePoint = GameObject.FindWithTag("WorkerIdlePoint")?.transform;

        if (counterOrderPoint == null) Debug.LogError($"WorkerAI [{instanceId}]: CounterOrderPoint not found! Check tag on scene object.");
        if (kitchenPrepPoint == null) Debug.LogError($"WorkerAI [{instanceId}]: KitchenPrepPoint not found! Check tag on scene object.");
        if (workerIdlePoint == null) Debug.LogError($"WorkerAI [{instanceId}]: WorkerIdlePoint not found! Check tag on scene object.");

        stove = FindObjectOfType<Stove>();
        oven = FindObjectOfType<Oven>();
        cuttingBoard = FindObjectOfType<CuttingBoard>();
        refrigerator = FindObjectOfType<Refrigerator>();

        if (stove == null) Debug.LogWarning($"WorkerAI [{instanceId}]: Stove not found in scene! Drag the stove object to the Stove field in WorkerAI Inspector.");
        if (oven == null) Debug.LogWarning($"WorkerAI [{instanceId}]: Oven not found in scene! Drag the oven object to the Oven field in WorkerAI Inspector.");
        if (cuttingBoard == null) Debug.LogWarning($"WorkerAI [{instanceId}]: CuttingBoard not found in scene! Drag the cutting board object to the CuttingBoard field in WorkerAI Inspector.");
        if (refrigerator == null) Debug.LogWarning($"WorkerAI [{instanceId}]: Refrigerator not found in scene! Drag the refrigerator object to the Refrigerator field in WorkerAI Inspector.");

        StartCoroutine(WorkerJobLoop());
        Debug.Log($"WorkerAI [{instanceId}]: WorkerJobLoop started.");
        CurrentWorkerTask = "Idle"; 
    }

    public class Order
    {
        public CustomerAI customer;
        public Recipe recipe;
        public TableSeatPoint customerSeat;
    }

    public TableSeatPoint ReceiveOrder(CustomerAI customer, Recipe recipe)
    {
        IsTakingOrder = true;
        TableSeatPoint freeSeat = null;
        if (GameManager.Instance != null && GameManager.Instance.customerManager != null)
        {
            freeSeat = GameManager.Instance.customerManager.GetFreeTableSeat();
        }
        else
        {
            Debug.LogError($"WorkerAI [{instanceId}]: GameManager.Instance or CustomerManager is null when trying to get free seat!");
        }

        if (freeSeat != null)
        {
            Debug.Log($"WorkerAI [{instanceId}]: Taking order from {customer.name} for {recipe.recipeName}. Assigning table {freeSeat.name}.");
            try
            {
                orderQueue.Enqueue(new Order { customer = customer, recipe = recipe, customerSeat = freeSeat });
                Debug.Log($"WorkerAI [{instanceId}]: Order added to queue. Current queue size: {orderQueue.Count}");
            }
            catch (Exception e)
            {
                Debug.LogError($"WorkerAI [{instanceId}]: Error enqueuing order: {e.Message}");
            }
            IsTakingOrder = false;
            CurrentWorkerTask = "Taking Order"; 
            return freeSeat;
        }
        else
        {
            Debug.LogWarning($"WorkerAI [{instanceId}]: No free tables for customer. Rejecting order.");
            IsTakingOrder = false;
            CurrentWorkerTask = "Idle"; 
            return null;
        }
    }

    private IEnumerator WorkerJobLoop()
    {
        while (true)
        {
            Debug.Log($"WorkerAI [{instanceId}]: WorkerJobLoop checking queue. Current size: {orderQueue.Count}");
            if (orderQueue.Count > 0)
            {
                currentOrder = orderQueue.Dequeue();
                Debug.Log($"WorkerAI [{instanceId}]: Dequeued order for {currentOrder.customer.name} - {currentOrder.recipe.recipeName}. Starting ProcessOrder.");
                yield return StartCoroutine(ProcessOrder(currentOrder));
                currentOrder = null;
                Debug.Log($"WorkerAI [{instanceId}]: Finished processing order. Checking for next order.");
                CurrentWorkerTask = "Idle"; 
            }
            else
            {
                if (workerIdlePoint != null)
                {
                    
                    if (Vector3.Distance(transform.position, workerIdlePoint.position) > agent.stoppingDistance + 0.1f || !agent.hasPath)
                    {
                        agent.SetDestination(workerIdlePoint.position);
                        agent.isStopped = false; 
                        Debug.Log($"WorkerAI [{instanceId}]: Moving to idle point.");
                        CurrentWorkerTask = "Moving to Idle Point"; 
                        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
                        agent.isStopped = true; 
                        Debug.Log($"WorkerAI [{instanceId}]: Arrived at idle point, waiting for orders.");
                        CurrentWorkerTask = "Idle"; 
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator ProcessOrder(Order order)
    {
        if (order == null || order.customer == null || order.recipe == null || order.customerSeat == null)
        {
            Debug.LogError($"WorkerAI [{instanceId}]: ProcessOrder received a null order or null components within the order!");
            CurrentWorkerTask = "Error"; 
            yield break;
        }

        Debug.Log($"WorkerAI [{instanceId}]: Starting to process order for {order.customer.name}: {order.recipe.recipeName}");
        CurrentWorkerTask = $"Processing {order.recipe.recipeName}"; 

        if (counterOrderPoint != null && Vector3.Distance(transform.position, counterOrderPoint.position) > agent.stoppingDistance + 0.1f)
        {
            agent.SetDestination(counterOrderPoint.position);
            agent.isStopped = false; 
            Debug.Log($"WorkerAI [{instanceId}]: Moving to counter to confirm order.");
            CurrentWorkerTask = "Moving to Counter"; 
            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
            agent.isStopped = true; 
            Debug.Log($"WorkerAI [{instanceId}]: Arrived at counter.");
        }

        if (kitchenPrepPoint != null)
        {
            agent.SetDestination(kitchenPrepPoint.position);
            agent.isStopped = false; // Ensure agent is not stopped when moving
            Debug.Log($"WorkerAI [{instanceId}]: Moving to kitchen, preparing to cook.");
            CurrentWorkerTask = "Moving to Kitchen"; 
            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
            agent.isStopped = true; // Stop agent when arrived
            Debug.Log($"WorkerAI [{instanceId}]: Arrived at kitchen prep point.");
        }
        else
        {
            Debug.LogError($"WorkerAI [{instanceId}]: KitchenPrepPoint not found! Cannot cook.");
            CurrentWorkerTask = "Error"; 
            yield break;
        }

        CraftingManager craftingManager = GameManager.Instance.craftingManager;
        if (craftingManager != null)
        {
            float currentCraftTime = order.recipe.craftTime;
            float speedMultiplier = 1.0f;

            if (stove != null)
            {
                speedMultiplier = stove.CurrentSpeedMultiplier;
                Debug.Log($"WorkerAI [{instanceId}]: Stove speed multiplier: {speedMultiplier}");
            }

            currentCraftTime /= speedMultiplier;

            Debug.Log($"WorkerAI [{instanceId}]: Attempting to start crafting '{order.recipe.recipeName}'.");
            CurrentWorkerTask = $"Crafting {order.recipe.recipeName}"; 
            if (craftingManager.StartCrafting(order.recipe))
            {
                Debug.Log($"WorkerAI [{instanceId}]: Cooking '{order.recipe.recipeName}' for {currentCraftTime:F2} seconds (multiplier applied).");
                yield return new WaitForSeconds(currentCraftTime);
                Debug.Log($"WorkerAI [{instanceId}]: Cooking of '{order.recipe.recipeName}' finished.");
            }
            else
            {
                Debug.LogWarning($"WorkerAI [{instanceId}]: Could not start cooking {order.recipe.recipeName}. Check ingredients or other crafting conditions.");
                CurrentWorkerTask = "Idle (No Ingredients)"; 
                yield break;
            }
        }
        else
        {
            Debug.LogError($"WorkerAI [{instanceId}]: CraftingManager not found! Cannot cook.");
            CurrentWorkerTask = "Error"; 
            yield break;
        }

        if (order.customerSeat != null)
        {
            agent.SetDestination(order.customerSeat.transform.position);
            agent.isStopped = false; 
            Debug.Log($"WorkerAI [{instanceId}]: Moving to customer {order.customer.name} at table {order.customerSeat.name} for delivery.");
            CurrentWorkerTask = $"Delivering {order.recipe.resultItem.itemName}"; 
            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
            agent.isStopped = true; 
            Debug.Log($"WorkerAI [{instanceId}]: Arrived at customer's table.");
        }
        else
        {
            Debug.LogError($"WorkerAI [{instanceId}]: No customer seat for order delivery! Cannot deliver order.");
            CurrentWorkerTask = "Error"; 
            yield break;
        }

        order.customer.DeliverOrder(order.recipe.resultItem);
        Debug.Log($"WorkerAI [{instanceId}]: Order delivered to customer.");
        CurrentWorkerTask = "Idle"; 
    }
}
