using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DescribeModeController : MonoBehaviour
{
    public Button describeButton;
    public Button educationButton;
    public TextMeshProUGUI educationButtonText;
    public Vector3 describeOffset = new Vector3(0, -0.7f, 0.8f);
    public Canvas describeCanvas;
    public CanvasGroup describeCanvasGroup;
    public float fadeDuration = 0.5f;
    public float fadeInDelay = 0.2f;
    public float resetOffsetDelay = 0.2f;

    private bool isDescribeMode = false;
    private Coroutine fadeCoroutine;

    public bool IsDescribeModeActive()
    {
        return isDescribeMode;
    }

    private void Start()
    {
        if (describeButton != null) describeButton.onClick.AddListener(DescribeButtonClicked);
        if (educationButton != null) educationButton.onClick.AddListener(ToggleDescribeMode);
        UpdateEducationButtonText();
        InitializeDescribeCanvas();
    }

    private void InitializeDescribeCanvas()
    {
        if (describeCanvas != null && describeCanvasGroup != null)
        {
            describeCanvas.gameObject.SetActive(false);
            describeCanvasGroup.alpha = 0f;
        }
        else
        {
            Debug.LogWarning("Describe Canvas or CanvasGroup is not set in DescribeModeController script!");
        }
    }

    private void Update()
    {
        // Input Manager
        if (!InputManager.Instance.IsInputAllowed())
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleDescribeMode();
        }
    }

    private void DescribeButtonClicked() {
        isDescribeMode = true;
        ActivateDescribeMode();
        UpdateEducationButtonText();
        Debug.Log("Describe Mode: " + (isDescribeMode ? "On" : "Off"));
    }

    private void ToggleDescribeMode()
    {
        isDescribeMode = !isDescribeMode;
        if (isDescribeMode)
        {
            ActivateDescribeMode();
        }
        else
        {
            StartCoroutine(DeactivateDescribeMode());
        }
        UpdateEducationButtonText();
        Debug.Log("Describe Mode: " + (isDescribeMode ? "On" : "Off"));
    }

    private void ActivateDescribeMode()
    {
        UpdateDescribeCanvasVisibility(true);
    }

    private IEnumerator DeactivateDescribeMode()
    {
        UpdateDescribeCanvasVisibility(false);
        yield return new WaitForSeconds(resetOffsetDelay);
    }

    private void UpdateEducationButtonText()
    {
        if (educationButtonText != null)
        {
            educationButtonText.text = isDescribeMode ? "Close Description (E)" : "Open Description (E)";
        }
        else
        {
            Debug.LogWarning("Description Button Text (TMP) is not set in DescribeModeController script!");
        }
    }

    private void UpdateDescribeCanvasVisibility(bool show)
    {
        if (describeCanvas != null && describeCanvasGroup != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (show)
            {
                describeCanvas.gameObject.SetActive(true);
                fadeCoroutine = StartCoroutine(FadeCanvas(0f, 1f, fadeInDelay));
            }
            else
            {
                fadeCoroutine = StartCoroutine(FadeCanvas(1f, 0f, 0f));
            }
        }
        else
        {
            Debug.LogWarning("Describe Canvas or CanvasGroup is not set in DescribeModeController script!");
        }
    }

    private IEnumerator FadeCanvas(float startAlpha, float endAlpha, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsedTime = 0f;
        describeCanvasGroup.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            describeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        describeCanvasGroup.alpha = endAlpha;

        if (endAlpha == 0f)
        {
            describeCanvas.gameObject.SetActive(false);
        }
    }
}