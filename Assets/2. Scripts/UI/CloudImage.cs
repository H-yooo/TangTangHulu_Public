using UnityEngine;

public class CloudImage : MonoBehaviour
{
    public float moveSpeed = 100f;  // 이동 속도

    private RectTransform cloudImageRect;

    private void Start()
    {
        cloudImageRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (cloudImageRect != null)
        {
            cloudImageRect.anchoredPosition += Vector2.right * moveSpeed * Time.deltaTime;
        }
    }
}
