using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiddleBossStageManager : GenericSingleton<MiddleBossStageManager>
{
    public int targetTanghuluCount;
    public int targetTanghuluGold;
    public int deliveredTanghuluCount = 0;
    public int additionalTanghuluGold;

    [SerializeField] private MatchFruit matchFruit;
    [SerializeField] private GameObject middleBossStagePanel;
    [SerializeField] private TextMeshProUGUI middleBossStageText;
    [SerializeField] private GameObject tanghuluContainer;
    [SerializeField] private Transform tanghuluContainerPosition;

    public Button middleBossStageUpgradeBtn;

    private int rowCount = 8; // 초기 행 수 초기화 (첫 행은 8개)
    private int currentCountInRow = 0; // 현재 행에 있는 탕후루 수

    private int tanghuluStickOrderinLayer = 13;
    private int fruitOrderinLayer = 14;
    private int papercupOrderinLayer = 15;

    public bool isMiddleBossStage = false;

    public void OnMiddleBossStageButtonClicked(int index)
    {
        isMiddleBossStage = true;

        switch (index)
        {
            case 0: StartMiddleBossStage(index); break;
            case 1: StartMiddleBossStage(index); break;
            case 2: StartMiddleBossStage(index); break;
            case 3: StartMiddleBossStage(index); break;
            case 4: StartMiddleBossStage(index); break;
            default: StartMiddleBossStage(index); break; // 기본값 (예외 처리)
        }
    }

    public Transform GetTanghuluContainerPosition()
    {
        return tanghuluContainerPosition;
    }

    public void AddDeliverdTanghuluCount()
    {
        deliveredTanghuluCount++;
    }

    public void StartMiddleBossStage(int index)
    {
        rowCount = 8;
        currentCountInRow = 0;

        Tanghulu.Instance.AdjustMatch3(); // 3Match로 전환
        matchFruit.ShowFruitToMatch();
        middleBossStagePanel.SetActive(true);
        tanghuluContainer.SetActive(true);
        SetTargetTanghuluCount(index);
        deliveredTanghuluCount = 0;
        UpdateMiddleBossStageUI();

        VillageManager.Instance.OnOpenShop();

        SoundManager.Instance.PlayBGM("MiddleBossStage");
    }

    private void SetTargetTanghuluCount(int index)
    {
        switch (index)
        {
            case 0: targetTanghuluCount = 10; targetTanghuluGold = 100; additionalTanghuluGold = 200; break;
            case 1: targetTanghuluCount = 12; targetTanghuluGold = 300; additionalTanghuluGold = 400; break;
            case 2: targetTanghuluCount = 14; targetTanghuluGold = 500; additionalTanghuluGold = 600; break;
            case 3: targetTanghuluCount = 17; targetTanghuluGold = 700; additionalTanghuluGold = 800; break;
            case 4: targetTanghuluCount = 20; targetTanghuluGold = 1000; additionalTanghuluGold = 1200; break;
            default: targetTanghuluCount = 10; targetTanghuluGold = 100; additionalTanghuluGold = 200; break; // 기본값 (예외 처리)
        }
    }

    public void UpdateMiddleBossStageUI()
    {
        middleBossStageText.text = $"납품수량 : {deliveredTanghuluCount} / {targetTanghuluCount}";
    }

    // Tanghulu를 피라미드 형태로 쌓기 위해 위치를 계산하는 메서드
    public Vector3 CalculateTanghuluPosition()
    {
        float baseSpacing = 0.9f; // Tanghulu 간의 기본 간격
        float initialX = -0.75f; // 시작 X 위치

        // X 위치 계산
        float offsetX;
        if (rowCount < 8) // 두 번째 줄 이후의 배치
        {
            // 이전 줄의 첫 번째 Tanghulu 위치 보정
            float rowOffset = (8 - rowCount) * (baseSpacing / 2);
            offsetX = initialX + rowOffset + (currentCountInRow * baseSpacing);
        }
        else
        {
            // 첫 번째 줄은 기본적인 Tanghulu 배치
            offsetX = initialX + (currentCountInRow * baseSpacing);
        }

        return new Vector3(offsetX, -0.5f, 0);
    }

    // Tanghulu 오브젝트와 자식들의 레이어 순서를 설정하는 메서드
    public void ApplyLayerOrder(GameObject tanghulu)
    {
        SpriteRenderer stickSpriteRenderer = tanghulu.GetComponent<SpriteRenderer>();
        if (stickSpriteRenderer != null)
        {
            stickSpriteRenderer.sortingOrder = tanghuluStickOrderinLayer;
        }

        SpriteRenderer[] childSpriteRenderers = tanghulu.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in childSpriteRenderers)
        {
            // Papercup 설정
            if (spriteRenderer.name.Contains("Papercup"))
            {
                spriteRenderer.sortingOrder = papercupOrderinLayer;
            }

            // 과일의 메인 Sprite 설정
            else if (spriteRenderer.name.Contains("MainSprite"))
            {
                spriteRenderer.sortingOrder = fruitOrderinLayer;
            }
            // Cherry_Stick과 같은 과일의 부모 오브젝트 설정
            else if (spriteRenderer.name.Contains("Stick"))
            {
                // 이 경우에는 과일의 부모 오브젝트일 수 있습니다.
                // 자식 중 MainSprite가 있는지 확인하여 그 sortingOrder 변경
                SpriteRenderer[] grandChildRenderers = spriteRenderer.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer grandChild in grandChildRenderers)
                {
                    if (grandChild.name.Contains("MainSprite"))
                    {
                        grandChild.sortingOrder = fruitOrderinLayer;
                    }
                }
            }
        }
    }

    // Tanghulu 위치 상태 업데이트 메서드 (다음 행으로 넘어가는 상태 업데이트)
    public void UpdateTanghuluPositionState()
    {
        currentCountInRow++;

        // 현재 행이 꽉 찼으면 다음 행으로 이동
        if (currentCountInRow >= rowCount)
        {
            currentCountInRow = 0; // 행 초기화
            rowCount--; // 행 수 감소 (아래로 갈수록 줄어드는 피라미드 형태)
            tanghuluStickOrderinLayer++; // 다음 줄의 레이어 순서 증가
            fruitOrderinLayer++;
            papercupOrderinLayer++;
        }
    }


    public void ResetTanghuluSortingOrders()
    {
        foreach (Transform tanghuluStick in tanghuluContainer.transform)
        {
            // TanghuluStick의 SpriteRenderer를 초기 상태로 복원
            SpriteRenderer stickSpriteRenderer = tanghuluStick.GetComponent<SpriteRenderer>();
            if (stickSpriteRenderer != null)
            {
                stickSpriteRenderer.sortingOrder = 13; // TanghuluStick 기본값으로 설정
            }

            // TanghuluStick 자식들의 SpriteRenderer도 초기 상태로 복원
            SpriteRenderer[] childSpriteRenderers = tanghuluStick.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in childSpriteRenderers)
            {
                if (spriteRenderer.name.Contains("Fruit"))
                {
                    spriteRenderer.sortingOrder = 14; // 과일 기본값
                }
                else if (spriteRenderer.name.Contains("Papercup"))
                {
                    spriteRenderer.sortingOrder = 15; // Papercup 기본값
                }
            }
        }

        // 초기 상태로 OrderInLayer 리셋
        tanghuluStickOrderinLayer = 13;
        fruitOrderinLayer = 14;
        papercupOrderinLayer = 15;
    }

    // Collider 및 Rigidbody 제거 메서드
    public void RemoveAllCollidersAndRigidbodies(Transform parent)
    {
        // 부모 오브젝트에서 Collider2D 및 Rigidbody2D 제거
        BoxCollider2D collider = parent.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Rigidbody2D rigidbody = parent.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            Destroy(rigidbody);
        }

        // 모든 자식들을 순회하며 Collider2D 및 Rigidbody2D 제거
        foreach (Transform child in parent)
        {
            RemoveAllCollidersAndRigidbodies(child); // 재귀 호출을 통해 모든 자식들까지 순회
        }
    }

    public void ClearTanghuluContainer()
    {
        foreach (Transform child in tanghuluContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void EndMiddleBossStage()
    {
        deliveredTanghuluCount = 0;
        ClearTanghuluContainer();
        middleBossStagePanel.SetActive(false);
        tanghuluContainer.SetActive(false);
        ResetTanghuluSortingOrders();
        Tanghulu.Instance.ChangeAdjustMatch(); // 원래 상태로 전환
        
        isMiddleBossStage = false;
    }
}