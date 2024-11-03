using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform
    public Vector3 offset = new Vector3(0, 0, 0.8f); // 플레이어와의 기본 거리 조절
    public float smoothSpeed = 5.0f; // 부드러운 이동을 위한 속도

    private Vector3 currentOffset;

    private void Start()
    {
        currentOffset = offset;
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference is not set in FollowPlayer script!");
            return;
        }

        // 플레이어 기준으로 회전이 적용된 오프셋 계산
        Vector3 rotatedOffset = player.TransformDirection(currentOffset);

        // 목표 위치 계산
        Vector3 targetPosition = player.position + rotatedOffset;

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Query Canvas가 플레이어를 바라보도록 하고 y축으로 180도 회전
        transform.LookAt(player);
        transform.Rotate(0, 180, 0, Space.Self);
    }

    public void SetOffset(Vector3 newOffset)
    {
        currentOffset = newOffset;
    }

    public void ResetOffset()
    {
        currentOffset = offset;
    }
}