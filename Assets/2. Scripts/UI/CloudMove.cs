using UnityEngine;

public class CloudMove : MonoBehaviour
{
    public GameObject cloudImagePrefab;
    public float spawnPositionX = 280f;
    public float destroyPositionX = 1720f;

    private RectTransform cloudRect; // Cloud�� RectTransform
    private bool hasCreatedNewCloud = false; // ���ο� Cloud ���� ���� ����

    private void Start()
    {
        cloudRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // ù ��° �ڽ��� CloudImage�� �����ϸ�
        if (transform.childCount > 0)
        {
            RectTransform cloudImageRect = transform.GetChild(0).GetComponent<RectTransform>();

            // CloudImage�� spawnPositionX�� �����ϸ� ���ο� Cloud ����
            if (cloudImageRect.anchoredPosition.x >= spawnPositionX && !hasCreatedNewCloud)
            {
                CreateNewCloudImage();
                hasCreatedNewCloud = true;  // ���ο� Cloud�� �����Ǿ����Ƿ� �÷��׸� true�� ����
            }

            // CloudImage�� destroyPositionX�� �����ϸ� ����
            if (cloudImageRect.anchoredPosition.x >= destroyPositionX)
            {
                Destroy(cloudImageRect.gameObject);
                hasCreatedNewCloud = false;  // ���� �� �ٽ� ���ο� Cloud�� ������ �� �ֵ��� �÷��� ����
            }
        }
    }

    private void CreateNewCloudImage()
    {
        // ���ο� CloudImage ����
        GameObject newCloudImageObject = Instantiate(cloudImagePrefab, transform);

        // ���ο� CloudImage�� ��ġ �ʱ�ȭ
        RectTransform newCloudImageRect = newCloudImageObject.GetComponent<RectTransform>();
        newCloudImageRect.anchoredPosition = new Vector2(-1720f, newCloudImageRect.anchoredPosition.y);
    }
}