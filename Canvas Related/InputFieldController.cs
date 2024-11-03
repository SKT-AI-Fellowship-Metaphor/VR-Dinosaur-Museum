using UnityEngine;
using TMPro;

public class InputFieldController : MonoBehaviour
{
    public TMP_InputField inputField;
    public PlayerMovement playerMovement;
    public OpenQuery openQuery;
    public ResetPlayerPosition resetPlayerPosition;

    private bool isInputFieldActive = false;
    private float blinkTimer = 0f;
    private bool showCursor = true;

    void Start()
    {
        inputField.onSelect.AddListener(OnInputFieldSelect);
        inputField.onDeselect.AddListener(OnInputFieldDeselect);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInputField();
        }

        if (isInputFieldActive)
        {
            BlinkCursor();
        }
    }

    void ToggleInputField()
    {
        isInputFieldActive = !isInputFieldActive;
        inputField.interactable = isInputFieldActive;

        if (isInputFieldActive)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
        else
        {
            inputField.DeactivateInputField();
        }

        // 다른 입력을 비활성화/활성화
        playerMovement.enabled = !isInputFieldActive;
        openQuery.enabled = !isInputFieldActive;
        resetPlayerPosition.enabled = !isInputFieldActive;
    }

    void BlinkCursor()
    {
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= 0.5f)
        {
            showCursor = !showCursor;
            blinkTimer = 0f;
        }

        string currentText = inputField.text;
        if (showCursor)
        {
            inputField.text = currentText + "|";
        }
        else
        {
            if (currentText.EndsWith("|"))
            {
                inputField.text = currentText.Substring(0, currentText.Length - 1);
            }
        }
    }

    void OnInputFieldSelect(string value)
    {
        isInputFieldActive = true;
    }

    void OnInputFieldDeselect(string value)
    {
        isInputFieldActive = false;
    }
}