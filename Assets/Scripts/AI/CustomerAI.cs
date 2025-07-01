using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class CustomerAI : MonoBehaviour
{
    public NavMeshAgent agent;
    private Transform targetPoint;
    private TableSeatPoint assignedTableSeat;
    private WorkerAI worker;
    private Recipe orderedRecipe;

    [Header("AI Settings")]
    [SerializeField] private float orderWaitTime = 2f;
    [SerializeField] private float eatTime = 5f;
    [SerializeField] private float minMoneyReward = 10f;
    [SerializeField] private float maxMoneyReward = 30f;
    [SerializeField] private int minXPReward = 5;
    [SerializeField] private int maxXPReward = 15;
    [SerializeField] private float sittingOffset = 0.4f;

    public bool IsOccupied { get; private set; } = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("CustomerAI: NavMeshAgent not found on object!");
        }
        agent.speed = 1.5f;
        agent.stoppingDistance = 0.5f;
    }

    public void Initialize(WorkerAI assignedWorker)
    {
        worker = assignedWorker;
        gameObject.SetActive(true);
        StartCoroutine(CustomerLifecycle());
    }

    private IEnumerator CustomerLifecycle()
    {
        Transform counterPoint = GameObject.FindWithTag("CounterOrderPoint")?.transform;
        if (counterPoint != null)
        {
            Debug.Log("Customer: Going to counter.");
            agent.SetDestination(counterPoint.position);
            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
        }
        else
        {
            Debug.LogError("CounterOrderPoint not found! Customer leaving cafe.");
            GoToExit();
            yield break;
        }

        Debug.Log("Customer: Waiting for worker to take order...");
        yield return new WaitUntil(() => worker != null && !worker.IsTakingOrder);

        List<Recipe> allRecipes = Resources.LoadAll<Recipe>("Recipes").ToList();

        Recipe coffeeRecipe = allRecipes.FirstOrDefault(r => r.name == "CoffeeRecipe");

        if (coffeeRecipe != null)
        {
            orderedRecipe = coffeeRecipe;
            Debug.Log("Customer: Prioritizing ordering Coffee.");
        }
        else if (allRecipes.Count > 0)
        {
            orderedRecipe = allRecipes[Random.Range(0, allRecipes.Count)];
            Debug.Log("Customer: Coffee recipe not found or available, ordering a random recipe.");
        }
        else
        {
            Debug.LogError("No recipes available! Customer leaving cafe.");
            GoToExit();
            yield break;
        }

        TableSeatPoint assignedSeat = null;
        if (worker != null)
        {
            Debug.Log($"Customer: Requesting order from worker {worker.name} for {orderedRecipe.recipeName}.");
            assignedSeat = worker.ReceiveOrder(this, orderedRecipe);
        }

        if (assignedSeat == null)
        {
            Debug.LogWarning("Worker could not assign a table. Customer leaving cafe.");
            GoToExit();
            yield break;
        }
        assignedTableSeat = assignedSeat;

        Debug.Log($"Customer: Ordering {orderedRecipe.recipeName}. Going to table {assignedTableSeat.name}");
        IsOccupied = true;

        agent.SetDestination(assignedTableSeat.transform.position);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

        agent.enabled = false;
        transform.position = assignedTableSeat.transform.position;
        Debug.Log("Customer: Sat down, waiting for order.");

        yield return new WaitUntil(() => receivedOrder);
        Debug.Log("Customer: Received order. Enjoying!");
        yield return new WaitForSeconds(eatTime);

        FinishOrder();

        agent.enabled = true;
        GoToExit();
    }

    private bool receivedOrder = false;

    public void DeliverOrder(Item item)
    {
        if (orderedRecipe != null && item.id == orderedRecipe.resultItem.id)
        {
            receivedOrder = true;
            Debug.Log($"Customer {name}: received {item.itemName}!");
        }
        else
        {
            Debug.LogWarning($"Customer {name}: Received wrong order, or order unknown.");
        }
    }

    private void FinishOrder()
    {
        double moneyReward = Random.Range(minMoneyReward, maxMoneyReward);
        int xpReward = Random.Range(minXPReward, maxXPReward);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoneyAndXP(moneyReward, xpReward);
        }
        else
        {
            Debug.LogError("GameManager.Instance not found!");
        }

        IsOccupied = false;
        if (assignedTableSeat != null)
        {
            assignedTableSeat.IsOccupied = false;
            Debug.Log($"Customer {name}: vacated table {assignedTableSeat.name}.");
        }
        Debug.Log($"Customer {name}: paid {moneyReward} money and gave {xpReward} XP. Leaving cafe.");
    }

    private void GoToExit()
    {
        Transform exitPoint = GameObject.FindWithTag("ExitPoint")?.transform;
        if (exitPoint != null)
        {
            Debug.Log("Customer: Going to exit.");
            agent.SetDestination(exitPoint.position);
            StartCoroutine(CheckExitDistance(exitPoint.position));
        }
        else
        {
            Debug.LogError("ExitPoint not found! Destroying customer object.");
            Destroy(gameObject);
        }
    }

    private IEnumerator CheckExitDistance(Vector3 exitPosition)
    {
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
        Debug.Log("Customer: Exited cafe.");
        Destroy(gameObject);
        if (GameManager.Instance != null && GameManager.Instance.customerManager != null)
        {
            GameManager.Instance.customerManager.CustomerExited();
        }
    }
}
