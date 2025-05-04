using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class ObjectiveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private ObjectiveTracker objectiveTracker;

    [Header("UI Settings")]
    [SerializeField] private string format = "{0}/{1} collected";
    [SerializeField] private Color completeColor = Color.green;
    [SerializeField] private Color inProgressColor = Color.white;

    private Canvas canvas;
    private RectTransform rectTransform;
    private Color defaultTextColor;

    private void Awake()
    {
        // Get the canvas component
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        // Set the canvas to screen space camera
        canvas.renderMode = RenderMode.ScreenSpaceCamera;

        // Position UI in bottom left corner
        SetupUIPosition();

        // Store default text color
        if (objectiveText != null)
        {
            defaultTextColor = objectiveText.color;
        }
    }

    private void Start()
    {
        if (objectiveTracker != null)
        {
            // Subscribe to the objective events
            objectiveTracker.OnObjectiveItemPicked += UpdateObjectiveText;
            objectiveTracker.OnObjectiveComplete += HandleObjectiveComplete;

            // Initialize UI with current values
            UpdateObjectiveText(objectiveTracker.GetCollectedCount(), objectiveTracker.GetTotalCount());
        }
        else
        {
            Debug.LogError("ObjectiveUI: No ObjectiveTracker found in the scene!");
        }
    }

    /// <summary>
    /// Position the UI in the bottom left corner of the screen
    /// </summary>
    private void SetupUIPosition()
    {
        if (rectTransform != null)
        {
            // Anchor to bottom left
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            // Position slightly away from the very edge
            rectTransform.anchoredPosition = new Vector2(10, 10);

            // Make sure the canvas has a reference to the camera
            if (Camera.main != null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }

    /// <summary>
    /// Updates the objective text to show current progress
    /// </summary>
    private void UpdateObjectiveText(int current, int total)
    {
        if (objectiveText != null)
        {
            objectiveText.text = string.Format(format, current, total);

            // Change color if complete
            objectiveText.color = (current >= total) ? completeColor : inProgressColor;
        }
    }

    /// <summary>
    /// Handle the objective complete event
    /// </summary>
    private void HandleObjectiveComplete()
    {
        // You could add additional UI effects here when all objectives are complete
        // For example, animation, sound effect, etc.
        if (objectiveText != null)
        {
            StartCoroutine(PulseTextColor());
        }
    }

    /// <summary>
    /// Simple animation to pulse the text color when objectives are complete
    /// </summary>
    private IEnumerator PulseTextColor()
    {
        float duration = 2.0f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 3, 1.0f);

            if (objectiveText != null)
            {
                objectiveText.color = Color.Lerp(completeColor, Color.white, t);
            }

            yield return null;
        }

        if (objectiveText != null)
        {
            objectiveText.color = completeColor;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (objectiveTracker != null)
        {
            objectiveTracker.OnObjectiveItemPicked -= UpdateObjectiveText;
            objectiveTracker.OnObjectiveComplete -= HandleObjectiveComplete;
        }
    }
}