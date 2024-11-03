using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideSelf : MonoBehaviour
{
    private void Start()
    {
        // 오브젝트를 비활성화하여 씬에서 숨깁니다.
        gameObject.SetActive(false);
    }
}
