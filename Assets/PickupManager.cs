using CodeMonkey.HealthSystemCM;
using UnityEngine;

public class PickupManager : MonoBehaviour
{

    void Start()
    {
        // Find all pickup items in the scene and connect them to the player
        PickupItem[] pickupItems = FindObjectsByType<PickupItem>(FindObjectsSortMode.None);

        foreach (PickupItem item in pickupItems)
        {
            // Subscribe to each pickup's OnPickup event
            item.OnPickup.AddListener(HealthSystem.HandlePickup);
            item.OnPickup.AddListener(EnergySystem.HandlePickup); // switch to ene
            item.OnPickup.AddListener(ObjectiveSystem.HandlePickup); // switch to object
        }
    }

    // Optional: Connect to pickups when they're spawned at runtime
    public void ConnectToPickup(PickupItem newPickup)
    {
        if (playerStats != null && newPickup != null)
        {
            newPickup.OnPickup.AddListener(playerStats.HandlePickup);
        }
    }
}