using UnityEngine;

public class CloudMove : MonoBehaviour
{
    public GameObject cloudImagePrefab;
    public float spawnPositionX = 280f;
    public float destroyPositionX = 1720f;

    private RectTransform cloudRect; // Cloud의 RectTransform
    private bool hasCreatedNewCloud = false; // 새로운 Cloud 생성 여부 추적

    private void Start()
    {
        cloudRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // 첫 번째 자식인 CloudImage가 존재하면
        if (transform.childCount > 0)
        {
            RectTransform cloudImageRect = transform.GetChild(0).GetComponent<RectTransform>();

            // CloudImage가 spawnPositionX에 도달하면 새로운 Cloud 생성
            if (cloudImageRect.anchoredPosition.x >= spawnPositionX && !hasCreatedNewCloud)
            {
                CreateNewCloudImage();
                hasCreatedNewCloud = true;  // 새로운 Cloud가 생성되었으므로 플래그를 true로 설정
            }

            // CloudImage가 destroyPositionX에 도달하면 삭제
            if (cloudImageRect.anchoredPosition.x >= destroyPositionX)
            {
                Destroy(cloudImageRect.gameObject);
                hasCreatedNewCloud = false;  // 삭제 후 다시 새로운 Cloud를 생성할 수 있도록 플래그 리셋
            }
        }
    }

    private void CreateNewCloudImage()
    {
        // 새로운 CloudImage 생성
        GameObject newCloudImageObject = Instantiate(cloudImagePrefab, transform);

        // 새로운 CloudImage의 위치 초기화
        RectTransform newCloudImageRect = newCloudImageObject.GetComponent<RectTransform>();
        newCloudImageRect.anchoredPosition = new Vector2(-1720f, newCloudImageRect.anchoredPosition.y);
    }
}