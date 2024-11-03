using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResetPlayerPosition : MonoBehaviour
{
    public CharacterController playerController;
    public Button resetButton;

    private Vector3 resetPosition = new Vector3(10.4f, 39.1f, 36.6f);
    private Vector3 resetRotation = new Vector3(0f, -90f, 0f); // -x 방향을 바라보도록 y축으로 180도 회전

    private void Start()
    {
        // 버튼에 리스너 추가
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetPosition);
        }
        else
        {
            Debug.LogError("Reset button is not assigned in the ResetPlayerPosition script.");
        }
    }

    private void Update()
    {
        // Input Manager
        if (!InputManager.Instance.IsInputAllowed())
        {
            return;
        }
        // R 키를 누르면 위치 초기화
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPosition();
        }
    }

    public void ResetPosition()
    {
        if (playerController != null)
        {
            // CharacterController를 비활성화
            playerController.enabled = false;

            // 플레이어 위치 재설정
            playerController.transform.position = resetPosition;

            // 플레이어 회전 재설정 (-x 방향을 바라보도록)
            playerController.transform.rotation = Quaternion.Euler(resetRotation);

            // CharacterController를 다시 활성화
            playerController.enabled = true;

            Debug.Log("Player position reset to: " + resetPosition + " and rotation reset to face -x direction.");
        }
        else
        {
            Debug.LogError("Player CharacterController is not assigned in the ResetPlayerPosition script.");
        }
    }
}