using UnityEngine;

public class FruitMovement : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float boundaryY = 1.5f; // 초록색과 파란색 부분의 경계 Y 값
    [SerializeField] private int fieldLayerOrder = 5; // 초록색 부분일 때 과일의 Order in Layer
    [SerializeField] private int fruitZoneLayerOrder = 17; // 파란색 부분일 때 과일의 Order in Layer

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        UpdateOrderInLayer();
    }

    private void UpdateOrderInLayer()
    {
        // 과일의 Y 위치를 기준으로 Order in Layer 변경
        if (transform.position.y > boundaryY)
        {
            spriteRenderer.sortingOrder = fieldLayerOrder;
        }
        else
        {
            spriteRenderer.sortingOrder = fruitZoneLayerOrder;
        }
    }
}
