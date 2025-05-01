using System.Collections.Generic;
using UnityEngine;

public class ItemHoldingSystem : MonoBehaviour
{
    [Header("Item Settings")]
    public Transform itemHoldPosition;      // Position where items appear on screen
    public GameObject heldItem;             // Reference to the current held item

    [Header("Swing Settings")]
    public float swingSpeed = 8f;           // Speed of swing animation (increased for more fluidity)
    public float swingAmount = 70f;         // Degree of rotation for swing (increased for more range)
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // For smoother animation
    public Vector3 swingRotationAxis = new Vector3(-1, 0.3f, 0.5f); // Custom swing axis for more natural feel

    [Header("Weapon Settings")]
    public bool isMeleeWeapon = true;       // Is this a melee weapon (sword, club, etc.)
    public float weaponDamage = 15f;        // Base damage for the weapon
    public float criticalHitChance = 0.1f;  // 10% chance for critical hit
    public float criticalHitMultiplier = 2f; // Damage multiplier for critical hits
    public LayerMask damageableLayers;      // Layers that can receive damage
    public float energyHitPenality = 3f;

    [Header("Collision Settings")]
    public float damageRadius = 1f;         // Radius to check for damage
    public Transform damageOrigin;          // Point from which to check for damage
    public bool visualizeHitbox = true;     // Debug option to see the hitbox

    // Private variables
    private bool isSwinging = false;
    private float swingTimer = 0f;
    private Quaternion originalItemRotation;
    private List<GameObject> damagedObjects = new List<GameObject>(); // Track already hit objects during swing
    private Vector3 currentSwingDirection;
    private bool damageDone = false;  // Track if damage has been applied in current swing
    private EnergySystem playerEnergySystem;

    [System.Serializable]
    public class DamageInfo
    {
        public GameObject target;
        public float damageAmount;
        public Vector3 hitPoint;
        public bool isCritical;
    }

    void Start()
    {
        // Store original rotation of the held item
        if (heldItem != null)
        {
            playerEnergySystem = GetComponentInParent<EnergySystemComponent>().GetEnergySystem();
            originalItemRotation = heldItem.transform.localRotation;
        }

        // If damageOrigin is not set, default to item position
        if (damageOrigin == null && heldItem != null)
        {
            damageOrigin = heldItem.transform;
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

            // Check for damage during the effective part of the swing
            if (isMeleeWeapon && swingTimer > 0.25f && swingTimer < 0.6f && !damageDone)
            {
                CheckForDamage();
            }
        }
    }

    void HandleItemInteraction()
    {
        // Check for left click or specified input
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            playerEnergySystem.EnergyDamage(energyHitPenality);
            StartSwingAnimation();
        }
    }

    public void StartSwingAnimation()
    {
        if (heldItem != null)
        {
            isSwinging = true;
            swingTimer = 0f;
            damagedObjects.Clear(); // Reset the list of damaged objects
            damageDone = false;

            // Randomize swing direction slightly for variety
            currentSwingDirection = new Vector3(
                swingRotationAxis.x + Random.Range(-0.1f, 0.1f),
                swingRotationAxis.y + Random.Range(-0.05f, 0.05f),
                swingRotationAxis.z + Random.Range(-0.1f, 0.1f)
            ).normalized;

            // Trigger swing start event
            OnSwingStart?.Invoke();
        }
    }

    void UpdateSwingAnimation()
    {
        if (heldItem == null) return;

        swingTimer += Time.deltaTime * swingSpeed;

        if (swingTimer <= 1f)
        {
            // Use animation curve for smoother motion
            float curveValue = swingCurve.Evaluate(swingTimer);

            // First half of swing (going forward)
            if (swingTimer <= 0.5f)
            {
                float t = curveValue * 2; // Normalize to 0-1 range
                float rotationAmount = Mathf.Lerp(0, swingAmount, t);

                // Apply rotation along custom axis for more natural swing
                heldItem.transform.localRotation = originalItemRotation *
                    Quaternion.AngleAxis(rotationAmount, currentSwingDirection);
            }
            // Second half of swing (going back)
            else
            {
                float t = (curveValue - 0.5f) * 2; // Normalize to 0-1 range
                float rotationAmount = Mathf.Lerp(swingAmount, 0, t);

                // Apply rotation along custom axis
                heldItem.transform.localRotation = originalItemRotation *
                    Quaternion.AngleAxis(rotationAmount, currentSwingDirection);
            }

            // Trigger mid-swing event at the apex of the swing
            if (swingTimer >= 0.45f && swingTimer <= 0.55f)
            {
                OnSwingMid?.Invoke();
            }
        }
        else
        {
            // Reset rotation and finish swing
            heldItem.transform.localRotation = originalItemRotation;
            isSwinging = false;

            // Trigger swing complete event
            OnSwingComplete?.Invoke();
        }
    }

    void CheckForDamage()
    {
        if (damageOrigin == null) return;

        // Check for objects in damage radius
        Collider[] hitColliders = Physics.OverlapSphere(damageOrigin.position, damageRadius, damageableLayers);

        bool hitSomething = false;

        foreach (Collider hitCollider in hitColliders)
        {
            GameObject hitObject = hitCollider.gameObject;

            // Skip if we've already damaged this object during this swing
            if (damagedObjects.Contains(hitObject))
                continue;

            // Add to damaged objects list
            damagedObjects.Add(hitObject);
            hitSomething = true;

            // Calculate damage
            float damage = CalculateDamage(hitObject);
            bool isCritical = Random.value <= criticalHitChance;
            if (isCritical)
            {
                damage *= criticalHitMultiplier;
            }

            // Create damage info
            DamageInfo damageInfo = new DamageInfo
            {
                target = hitObject,
                damageAmount = damage,
                hitPoint = hitCollider.ClosestPoint(damageOrigin.position),
                isCritical = isCritical
            };

            // Fire damage event
            OnDamageCaused?.Invoke(damageInfo);
        }

        if (hitSomething)
        {
            damageDone = true;
            OnWeaponHit?.Invoke();
        }
    }

    float CalculateDamage(GameObject target)
    {
        // Base implementation - can be expanded to include armor, resistances, etc.
        return weaponDamage;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the damage radius if enabled
        if (visualizeHitbox && damageOrigin != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(damageOrigin.position, damageRadius);
        }
    }

    // Events that other scripts can subscribe to
    public delegate void SwingEvent();
    public event SwingEvent OnSwingStart;
    public event SwingEvent OnSwingMid;
    public event SwingEvent OnSwingComplete;
    public event SwingEvent OnSwingCancel;
    public event SwingEvent OnWeaponHit;

    // Damage event with info
    public delegate void DamageEvent(DamageInfo damageInfo);
    public event DamageEvent OnDamageCaused;
}

// Optional helper class to identify weapon types
public class WeaponType : MonoBehaviour
{
    public bool isMeleeWeapon = true;
    public float baseDamage = 15f;
    public string weaponName = "Weapon";
}