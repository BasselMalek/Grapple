using CodeMonkey.HealthSystemCM;
using System;
using UnityEngine;

public class ObjectiveTracker : MonoBehaviour
{
    [Header("Objective Settings")]
    [SerializeField] private int totalObjectives = 5;
    [SerializeField] private int collectedObjectives = 0;

    // Events that other scripts can subscribe to
    public event Action<int, int> OnObjectiveItemPicked; // Current, Total
    public event Action OnObjectiveComplete;

    private bool isComplete = false;

    private void Start()
    {
        // Initialize the objectives
        ResetObjectives();
    }

    /// <summary>
    /// Resets the objective counter
    /// </summary>
    public void ResetObjectives()
    {
        collectedObjectives = 0;
        isComplete = false;

        // Notify UI about initial state
        OnObjectiveItemPicked?.Invoke(collectedObjectives, totalObjectives);
    }

    /// <summary>
    /// Called when player picks up an objective item
    /// </summary>
    public void HandlePickup()
    {
        // Increment counter
        collectedObjectives++;

        // Clamp to max value
        collectedObjectives = Mathf.Min(collectedObjectives, totalObjectives);

        // Notify subscribers about the pickup
        OnObjectiveItemPicked?.Invoke(collectedObjectives, totalObjectives);
        GetComponentInParent<EnergySystemComponent>().GetEnergySystem().RegenEnergy(10);
        GetComponentInParent<HealthSystemComponent>().GetHealthSystem().Heal(10);
        // Check if all objectives are collected
        CheckObjectiveCompletion();
    }

    /// <summary>
    /// Check if all objectives have been collected
    /// </summary>
    private void CheckObjectiveCompletion()
    {
        if (collectedObjectives >= totalObjectives && !isComplete)
        {
            isComplete = true;
            // Fire the objective complete event
            OnObjectiveComplete?.Invoke();
            Debug.Log("All objectives completed!");
        }
    }
    /// <summary>
    /// Get the current number of collected objectives
    /// </summary>
    public int GetCollectedCount()
    {
        return collectedObjectives;
    }

    /// <summary>
    /// Get the total number of objectives
    /// </summary>
    public int GetTotalCount()
    {
        return totalObjectives;
    }

}