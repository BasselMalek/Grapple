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
            // switch to ene
            if (item.pickupType == PickupType.Health)
            {
                item.OnPickup.AddListener(GetComponentInParent<HealthSystemComponent>().GetHealthSystem().HandlePickup);

            }
            else if (item.pickupType == PickupType.Energy)
            {

                item.OnPickup.AddListener(GetComponentInParent<EnergySystemComponent>().GetEnergySystem().HandlePickup);
            }
            else
            {
                item.OnPickup.AddListener(GetComponentInParent<ObjectiveTracker>().HandlePickup);
            }
        }
    }
}