using UnityEngine;
using UnityEngine.UI;

public class ButtonDeactivator : MonoBehaviour
{
    public Button[] buttonsToDeactivate;

    private void Start()
    {
        DeactivateButtons();
    }

    private void DeactivateButtons()
    {
        foreach (Button button in buttonsToDeactivate)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
                Debug.Log($"Deactivated button: {button.name}");
            }
            else
            {
                Debug.LogWarning("A button reference in ButtonDeactivator is null!");
            }
        }
    }

    public void ActivateButtons()
    {
        foreach (Button button in buttonsToDeactivate)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
                Debug.Log($"Activated button: {button.name}");
            }
            else
            {
                Debug.LogWarning("A button reference in ButtonDeactivator is null!");
            }
        }
    }
}