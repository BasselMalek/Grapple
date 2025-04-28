using UnityEngine;

public class EnergySystemComponent : MonoBehaviour, IGetEnergySystem
{
    [Tooltip("Maximum Energy amount")]
    [SerializeField] private float energyAmountMax = 100f;

    [Tooltip("Starting Energy amount, leave at 0 to start at full energy.")]
    [SerializeField] private float startingEnergyAmount;

    [Tooltip("Rate at which energy depletes automatically per second")]
    [SerializeField] private float depletionRate = 1f;

    [Tooltip("Rate at which energy regenerates per second (0 for no regeneration)")]
    [SerializeField] private float regenRate = 0f;

    [Tooltip("Should energy deplete automatically over time?")]
    [SerializeField] private bool autoDepleting = true;

    private EnergySystem energySystem;

    private void Awake()
    {
        // Create Energy System
        energySystem = new EnergySystem(energyAmountMax, depletionRate, regenRate);

        if (startingEnergyAmount != 0)
        {
            energySystem.SetEnergy(startingEnergyAmount);
        }

        energySystem.SetAutoDepleting(autoDepleting);
    }

    private void Update()
    {
        // Update the energy system with delta time for automatic depletion/regeneration
        energySystem.Update(Time.deltaTime);
    }

    /// <summary>
    /// Get the Energy System created by this Component
    /// </summary>
    public EnergySystem GetEnergySystem()
    {
        return energySystem;
    }
}