using UnityEngine;

public class ItemHoldingSystem : MonoBehaviour
{
    [Header("Item Settings")]
    public Transform itemHoldPosition; // Position where items appear on screen
    public GameObject heldItem; // Reference to the current held item
    public float swingSpeed = 5f; // Speed of swing animation
    public float swingAmount = 30f; // Degree of rotation for swing

    // Private variables
    private bool isSwinging = false;
    private float swingTimer = 0f;
    private Quaternion originalItemRotation;

    void Start()
    {
        // Store original rotation of the held item
        if (heldItem != null)
        {
            originalItemRotation = heldItem.transform.localRotation;
        }
    }

    void Update()
    {
        // Handle item interaction
        HandleItemInteraction();

        // Update swing animation if active
        if (isSwinging)
        {
            UpdateSwingAnimation();
        }
    }

    void HandleItemInteraction()
    {
        // Check for left click
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            StartSwingAnimation();
        }
    }

    public void StartSwingAnimation()
    {
        if (heldItem != null)
        {
            isSwinging = true;
            swingTimer = 0f;

            // You can add event triggers here
            OnSwingStart?.Invoke();
        }
    }

    void UpdateSwingAnimation()
    {
        if (heldItem == null) return;

        swingTimer += Time.deltaTime * swingSpeed;

        if (swingTimer <= 1f)
        {
            // First half of swing (going forward)
            if (swingTimer <= 0.5f)
            {
                float t = swingTimer * 2; // Normalize to 0-1 range
                float rotationAmount = Mathf.Lerp(0, swingAmount, t);
                heldItem.transform.localRotation = originalItemRotation * Quaternion.Euler(-rotationAmount, 0, 0);
            }
            // Second half of swing (going back)
            else
            {
                float t = (swingTimer - 0.5f) * 2; // Normalize to 0-1 range
                float rotationAmount = Mathf.Lerp(swingAmount, 0, t);
                heldItem.transform.localRotation = originalItemRotation * Quaternion.Euler(-rotationAmount, 0, 0);
            }

            // Trigger mid-swing event at the apex of the swing
            if (swingTimer >= 0.5f && swingTimer <= 0.5f + Time.deltaTime * swingSpeed)
            {
                OnSwingMid?.Invoke();
            }
        }
        else
        {
            // Reset rotation and finish swing
            heldItem.transform.localRotation = originalItemRotation;
            isSwinging = false;

            // You can add event triggers here
            OnSwingComplete?.Invoke();
        }
    }

    // Set the held item
    public void SetHeldItem(GameObject newItem)
    {
        // Clean up old item if it exists
        if (heldItem != null)
        {
            Destroy(heldItem);
        }

        // Set new item
        heldItem = newItem;

        if (heldItem != null)
        {
            // Ensure it's properly parented and positioned
            heldItem.transform.SetParent(itemHoldPosition, false);
            heldItem.transform.localPosition = Vector3.zero;
            heldItem.transform.localRotation = Quaternion.identity;

            // Store the original rotation
            originalItemRotation = heldItem.transform.localRotation;
        }
    }

    // Events that other scripts can subscribe to
    public delegate void SwingEvent();
    public event SwingEvent OnSwingStart;
    public event SwingEvent OnSwingMid;
    public event SwingEvent OnSwingComplete;
}