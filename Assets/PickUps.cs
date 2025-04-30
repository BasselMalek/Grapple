using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
/// <summary>
/// Main component for any pickup item in the game
/// </summary>
public class PickupItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public string pickupID = "item_01";               // Unique identifier for this pickup
    public string pickupName = "Item";                // Display name 
    public PickupType pickupType = PickupType.Health; // Type of pickup
    public float value = 10f;                         // Value (health points, energy amount, objective progress, etc.)
    public bool destroyOnPickup = true;               // Whether to destroy the object when picked up
    public float respawnTime = 0f;                    // If > 0, item will respawn after this many seconds

    [Header("Visual Settings")]
    public bool rotateItem = true;                    // Should the item rotate?
    public float rotationSpeed = 50f;                 // How fast it rotates
    public bool bobUpAndDown = true;                  // Should the item bob up and down?
    public float bobHeight = 0.2f;                    // How high the bob motion goes
    public float bobSpeed = 1f;                       // How fast the bob motion is

    [Header("Wobble Effect")]
    public bool enableWobble = true;                  // Should the item wobble in air?
    public float wobbleAmount = 15f;                  // Maximum angle of wobble
    public float wobbleSpeed = 2f;                    // Speed of wobble animation
    public Vector3 wobbleAxis = new Vector3(0.5f, 0, 0.5f); // Axis to wobble around
    public float wobblePhaseOffset = 0.5f;            // Phase offset between axes for more natural movement

    [Header("Effect Settings")]
    public GameObject pickupEffectPrefab;             // Visual effect spawned when item is picked up
    public AudioClip pickupSound;                     // Sound played when picked up
    public float pickupSoundVolume = 1f;              // Volume of the pickup sound

    [Header("Events")]
    public PickupEvent OnPickup = new PickupEvent();  // Event fired when item is picked up

    // Private variables
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Collider itemCollider;
    private Renderer itemRenderer;
    private bool isPickedUp = false;
    private float bobOffset;
    private float wobbleOffsetX;
    private float wobbleOffsetY;
    private float wobbleOffsetZ;

    void Start()
    {
        // Store initial position and rotation for effects and respawning
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Get components
        itemCollider = GetComponent<Collider>();
        itemRenderer = GetComponent<Renderer>();

        // Slight randomization of effect offsets for variety
        bobOffset = Random.Range(0f, 2f * Mathf.PI);
        wobbleOffsetX = Random.Range(0f, 2f * Mathf.PI);
        wobbleOffsetY = Random.Range(0f, 2f * Mathf.PI);
        wobbleOffsetZ = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        if (isPickedUp) return;

        // Apply all visual effects
        ApplyItemEffects();
    }

    /// <summary>
    /// Applies all visual effects to the item (rotation, bobbing, wobbling)
    /// </summary>
    void ApplyItemEffects()
    {
        // Calculate bobbing
        float yOffset = 0f;
        if (bobUpAndDown)
        {
            yOffset = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight;
        }

        // Apply position from bob effect
        transform.position = startPosition + new Vector3(0f, yOffset, 0f);

        // Reset rotation to starting point before applying effects
        Quaternion targetRotation = startRotation;

        // Apply wobble effect
        if (enableWobble)
        {
            // Create a wobble around all three axes with phase offsets for more natural movement
            float wobbleX = Mathf.Sin((Time.time + wobbleOffsetX) * wobbleSpeed) * wobbleAmount;
            float wobbleY = Mathf.Sin((Time.time + wobbleOffsetY) * wobbleSpeed + wobblePhaseOffset) * wobbleAmount;
            float wobbleZ = Mathf.Sin((Time.time + wobbleOffsetZ) * wobbleSpeed + wobblePhaseOffset * 2) * wobbleAmount;

            // Create wobble effect by applying small rotations along the wobble axis
            Vector3 wobbleVector = new Vector3(
                wobbleX * wobbleAxis.x,
                wobbleY * wobbleAxis.y,
                wobbleZ * wobbleAxis.z
            );

            targetRotation *= Quaternion.Euler(wobbleVector);
        }

        // Apply rotation effect (spinning)
        if (rotateItem)
        {
            targetRotation *= Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0);
            transform.rotation = targetRotation;
        }
        else if (enableWobble)
        {
            // If not rotating but wobbling, just apply the wobble rotation
            transform.rotation = targetRotation;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") && !isPickedUp)
        {
            PickupObject(other.gameObject);
        }
    }

    /// <summary>
    /// Handle the actual pickup logic
    /// </summary>
    public void PickupObject(GameObject player)
    {
        if (isPickedUp) return;
        isPickedUp = true;

        // Create pickup data to pass to listeners
        PickupData data = new PickupData(pickupType, value, gameObject, pickupID);

        // Trigger the pickup event
        OnPickup.Invoke();

        // Spawn pickup effect if assigned
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play pickup sound if assigned
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
        }

        // Hide the object temporarily or destroy it
        if (destroyOnPickup)
        {
            if (respawnTime > 0)
            {
                StartCoroutine(RespawnRoutine());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Respawn the item after a delay
    /// </summary>
    private IEnumerator RespawnRoutine()
    {
        // Hide the object
        if (itemRenderer != null) itemRenderer.enabled = false;
        if (itemCollider != null) itemCollider.enabled = false;

        // Wait for respawn time
        yield return new WaitForSeconds(respawnTime);

        // Show the object again
        if (itemRenderer != null) itemRenderer.enabled = true;
        if (itemCollider != null) itemCollider.enabled = true;

        // Reset position, rotation and state
        transform.position = startPosition;
        transform.rotation = startRotation;
        isPickedUp = false;

        // Re-randomize effect offsets for variety on respawn
        wobbleOffsetX = Random.Range(0f, 2f * Mathf.PI);
        wobbleOffsetY = Random.Range(0f, 2f * Mathf.PI);
        wobbleOffsetZ = Random.Range(0f, 2f * Mathf.PI);
    }
}

public class PickupEvent : UnityEvent { }

/// <summary>
/// Data container passed when a pickup is collected
/// </summary>
public class PickupData
{
    public PickupType pickupType;
    public float value;
    public GameObject pickupObject;
    public string pickupID;

    public PickupData(PickupType type, float val, GameObject obj, string id)
    {
        pickupType = type;
        value = val;
        pickupObject = obj;
        pickupID = id;
    }
}

/// <summary>
/// Defines the type of effect this pickup will have
/// </summary>
public enum PickupType
{
    Health,
    Energy,
    Objective,
}
