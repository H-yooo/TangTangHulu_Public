using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StageManager : GenericSingleton<StageManager>
{
    [SerializeField] private TextMeshProUGUI stageText;
    public int currentStageLevel = 1;

    [SerializeField] private GameObject fieldParent;
    [SerializeField] private List<GameObject> normalStagePrefabs;
    [SerializeField] private List<GameObject> middleBossStagePrefabs;
    [SerializeField] private List<GameObject> bossStagePrefabs;
    private GameObject currentFieldInstance;

    [SerializeField] List<GameObject> repeatImagePrefabs = new List<GameObject>(); // 반복 생성될 이미지 리스트
    private float initialSpawnHeight = 102.16f; // 초기화용 높이
    private float nextImageSpawnHeight; // 다음 이미지 생성 높이 (스테이지마다 갱신)
    private float repeatImageHeight = 30.4f; // 이미지 높이

    public void StartStage()
    {
        ChangeFieldImage();
        UpdateStageUI();

        if (BossStageManager.Instance.isBossStage == true)
        {
            nextImageSpawnHeight = initialSpawnHeight;
        }
        else
        {
            Tanghulu.Instance.ChangeAdjustMatch();
        }
    }

    public void UpdateStageUI()
    {
        stageText.text = "Day" + currentStageLevel;
        VillageManager.Instance.villageStageText.text = "Day" + currentStageLevel;
    }

    public void ChangeFieldImage()
    {
        if (currentFieldInstance != null)
        {
            Destroy(currentFieldInstance);
        }

        GameObject prefabToInstantiate = null;

        if (MiddleBossStageManager.Instance.isMiddleBossStage == true)
        {
            int index = (currentStageLevel / 10) % middleBossStagePrefabs.Count;
            prefabToInstantiate = middleBossStagePrefabs[index];
        }
        else if (BossStageManager.Instance.isBossStage == true)
        {
            int index = (currentStageLevel / 10) % bossStagePrefabs.Count;
            prefabToInstantiate = bossStagePrefabs[index];
        }
        else
        {
            int index = (currentStageLevel - 1) / 10;
            if (index >= 0 && index < normalStagePrefabs.Count)
            {
                prefabToInstantiate = normalStagePrefabs[index];
            }
        }

        if (prefabToInstantiate != null)
        {
            currentFieldInstance = Instantiate(prefabToInstantiate, fieldParent.transform);
            currentFieldInstance.transform.localPosition = Vector3.zero; // 위치 조정
        }
        else
        {
            Debug.LogWarning("프리팹을 찾을 수 없습니다.");
        }
    }

    private Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            // 원래 이름과 (Clone)이 붙은 이름 모두 비교
            if (child.name == childName || child.name == $"{childName}(Clone)")
            {
                return child;
            }
        }
        return null;
    }

    // 반복 이미지 생성 메서드
    public void SpawnRepeatedImage()
    {
        if (currentStageLevel % 10 == 0)
        {
            GameObject imagePrefab = repeatImagePrefabs[0];

            // 반복 이미지를 추가할 부모를 찾기
            int index = (currentStageLevel / 10) % bossStagePrefabs.Count;
            GameObject bossStagePrefab = bossStagePrefabs[index];
            Transform bossStageInstance = FindChildByName(fieldParent.transform, bossStagePrefab.name);

            if (bossStageInstance != null)
            {
                GameObject newImage = Instantiate(
                    imagePrefab,
                    Vector3.zero,
                    Quaternion.identity,
                    bossStageInstance
                );

                // 로컬 위치 강제 설정
                newImage.transform.localPosition = new Vector3(0, nextImageSpawnHeight, 0);

                // 다음 생성 위치 갱신
                nextImageSpawnHeight += repeatImageHeight;
            }
        }
    }
}
