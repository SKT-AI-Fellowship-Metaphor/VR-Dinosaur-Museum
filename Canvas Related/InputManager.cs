using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public List<TMP_InputField> managedInputFields = new List<TMP_InputField>();
    public PlayerMovement playerMovement;
    public ResetPlayerPosition resetPlayerPosition;

    private bool isAnyInputFieldFocused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var inputField in managedInputFields)
        {
            if (inputField != null)
            {
                inputField.onSelect.AddListener((string value) => OnInputFieldSelect(inputField));
                inputField.onDeselect.AddListener((string value) => OnInputFieldDeselect(inputField));
            }
        }
    }

    private void OnInputFieldSelect(TMP_InputField inputField)
    {
        isAnyInputFieldFocused = true;
        Debug.Log($"Input field focused: {inputField.name}");
    }

    private void OnInputFieldDeselect(TMP_InputField inputField)
    {
        isAnyInputFieldFocused = managedInputFields.Exists(field => field.isFocused);
        Debug.Log($"Input field defocused: {inputField.name}. Any field still focused: {isAnyInputFieldFocused}");
    }

    public bool IsInputAllowed()
    {
        return !isAnyInputFieldFocused;
    }

    // 새로운 입력 필드를 동적으로 추가하는 메서드
    public void AddInputField(TMP_InputField inputField)
    {
        if (!managedInputFields.Contains(inputField))
        {
            managedInputFields.Add(inputField);
            inputField.onSelect.AddListener((string value) => OnInputFieldSelect(inputField));
            inputField.onDeselect.AddListener((string value) => OnInputFieldDeselect(inputField));
            Debug.Log($"New input field added: {inputField.name}");
        }
    }

    // 입력 필드를 제거하는 메서드
    public void RemoveInputField(TMP_InputField inputField)
    {
        if (managedInputFields.Contains(inputField))
        {
            inputField.onSelect.RemoveListener((string value) => OnInputFieldSelect(inputField));
            inputField.onDeselect.RemoveListener((string value) => OnInputFieldDeselect(inputField));
            managedInputFields.Remove(inputField);
            Debug.Log($"Input field removed: {inputField.name}");
        }
    }
}