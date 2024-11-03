using UnityEngine;

public class TransparentSelf : MonoBehaviour
{
    private Material material;
    private Color originalColor;
    [SerializeField] private float targetAlpha = 0.001f;

    void Start()
    {
        // Renderer 컴포넌트 가져오기
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // 새로운 Material 인스턴스 생성하여 할당
            material = new Material(renderer.material);
            renderer.material = material;
            
            // 현재 색상 저장
            originalColor = material.color;
            
            // 알파값만 변경
            material.color = new Color(originalColor.r, originalColor.g, originalColor.b, targetAlpha);
        }
        else
        {
            Debug.LogWarning("Renderer component not found on " + gameObject.name);
        }
    }
}