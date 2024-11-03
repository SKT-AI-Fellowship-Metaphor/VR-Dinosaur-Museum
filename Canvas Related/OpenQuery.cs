using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction.Locomotion;

public class OpenQuery : MonoBehaviour
{
    [SerializeField] private GameObject queryCanvas;
    [SerializeField] private GameObject describeCanvas;
    [SerializeField] private Button queryButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private DescribeModeController describeModeController;
    
    [Header("Query Canvas Transparency")]
    [SerializeField] public float queryTransparency = 0.5f;
    
    private CanvasGroup queryCanvasGroup;

    private void Start()
    {
        // CanvasGroup 컴포넌트는 queryCanvas에만 적용
        if (queryCanvas != null)
        {
            queryCanvasGroup = queryCanvas.GetComponent<CanvasGroup>();
            if (queryCanvasGroup == null)
            {
                queryCanvasGroup = queryCanvas.AddComponent<CanvasGroup>();
            }
            queryCanvas.SetActive(false);
        }

        if (describeCanvas != null)
        {
            describeCanvas.SetActive(false);
        }
        UpdateButtonText();

        if (queryButton != null)
        {
            queryButton.onClick.AddListener(ToggleCanvases);
        }
    }

    private void Update()
    {
        if (!InputManager.Instance.IsInputAllowed())
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleCanvases();
        }
    }

    private void ToggleCanvases()
    {
        bool newState = !queryCanvas.activeSelf;

        if (queryCanvas != null)
        {
            queryCanvas.SetActive(newState);
            // 투명도는 queryCanvas에만 적용
            if (newState && queryCanvasGroup != null)
            {
                queryCanvasGroup.alpha = queryTransparency;
            }
        }

        // describeCanvas는 투명도 변경 없이 그대로 토글만 수행
        if (describeCanvas != null && describeModeController != null && describeModeController.IsDescribeModeActive())
        {
            describeCanvas.SetActive(newState);
        }

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = queryCanvas.activeSelf ? "Close Query (Q)" : "Open Query (Q)";
        }
    }
}