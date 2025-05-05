using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchFruit : MonoBehaviour
{
    [SerializeField] private Fever fever;
    [SerializeField] private Transform fruitPosition; // 과일이 들어갈 위치
    [SerializeField] private List<FruitType> currentFruits = new List<FruitType>();
    [SerializeField] private List<FruitType> matchedFruitList = new List<FruitType>();
    [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;

    [SerializeField] private GameObject matchFruitPanel;
    
    private int totalFruitPrice = 0;  // 과일 가격을 합산할 변수

    public bool IsFruitMatched(FruitType fruitType)
    {
        return currentFruits.Count > 0 && currentFruits[0] == fruitType;
    }

    // 탕후루 초기화 메서드
    public void InitializeTangHulu()
    {
        totalFruitPrice = 0;

        for (int i = 0; i < Tanghulu.Instance.CurrentFruitCount; i++)
        {
            AddRandomFruits();
        }
    }

    public void ResetFruit()
    {
        // 기존 과일 제거
        foreach (Transform child in fruitPosition)
        {
            Destroy(child.gameObject);
        }

        currentFruits.Clear();  // 리스트도 초기화

        // 새로운 과일 생성
        InitializeTangHulu();
    }

    // FruitManager에 등록된 프리팹 중 랜덤하게 과일 추가 메서드
    private void AddRandomFruits()
    {
        int randomIndex = Random.Range(0, FruitManager.Instance.CurrentFruitDataList.Count);
        FruitData fruitData = FruitManager.Instance.CurrentFruitDataList[randomIndex];

        if (fruitData != null && fruitData.fruitImage != null)
        {
            GameObject fruitImage = new GameObject("FruitImage");
            fruitImage.transform.SetParent(fruitPosition);

            Image imageComponent = fruitImage.AddComponent<Image>();
            imageComponent.sprite = fruitData.fruitImage.sprite; // UI이미지 설정

            RectTransform rectTransform = fruitImage.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            // rectTransform.sizeDelta = new Vector2(100, 100); // 필요시 사이즈 조정

            currentFruits.Add(fruitData.fruitType);
        }
        else
        {
            Debug.LogError("FruitData 또는 Fruit Image가 없습니다.");
        }
    }

    // 과일 맞췄을 때 호출될 메서드
    public void CheckMatch(FruitType selectedFruit)
    {
        if (currentFruits.Count > 0)
        {
            if (currentFruits[0] == selectedFruit)
            {
                FruitTouchSFX();

                // Fever 게이지 획득
                fever.IncreaseFeverGauge();

                // TanghuluStick에 과일 추가
                Tanghulu.Instance.AddFruitToTanghulu(selectedFruit);

                if (MiddleBossStageManager.Instance.isMiddleBossStage == true)
                {
                    TimeManager.Instance.TimerPlusMinusUI(true);
                }
                else if (BossStageManager.Instance.isBossStage == true)
                {
                    BossStageManager.Instance.AddTanghuluCount();
                    BossStageManager.Instance.SubCameraMoveUp();

                    if(fever.IsFeverGaugeFull())
                    {
                        fever.StartFeverMode();
                    }
                }
                else
                {
                    // 선택된 과일의 가격을 totalFruitPrice에 더하기
                    FruitData matchedFruitData = FruitManager.Instance.GetFruitDataByType(selectedFruit);
                    totalFruitPrice += FruitManager.Instance.GetFruitPrice(matchedFruitData);
                    TimeManager.Instance.TimerPlusMinusUI(true);
                }

                // 맞춘 과일 제거
                currentFruits.RemoveAt(0);
                Destroy(fruitPosition.GetChild(0).gameObject);

                // 보스스테이지 과일 추가
                if (BossStageManager.Instance.isBossStage == true)
                {
                    AddRandomFruits();
                }
                horizontalLayoutGroup.CalculateLayoutInputHorizontal();
                horizontalLayoutGroup.SetLayoutHorizontal();
            }
            else
            {
                // 틀린 과일을 선택했을 때 처리
                SoundManager.Instance.PlaySFX("FruitFail");

                if (MiddleBossStageManager.Instance.isMiddleBossStage == true)
                {
                    TimeManager.Instance.TimerPlusMinusUI(false);
                    Debug.LogWarning("틀린 과일을 선택했습니다. 순서를 지키세요.");
                }
                else if (BossStageManager.Instance.isBossStage == true)
                {
                    TimeManager.Instance.TimerPlusMinusUI(false);
                    Debug.LogWarning("틀린 과일을 선택했습니다. 순서를 지키세요.");
                }
                else
                {
                    TimeManager.Instance.TimerPlusMinusUI(false);

                    CustomerManager.Instance.DecreaseFavorability();

                    // TanghuluStick에 과일 추가
                    Tanghulu.Instance.AddFruitToTanghulu(selectedFruit);

                    // 과일 제거
                    currentFruits.RemoveAt(0);
                    Destroy(fruitPosition.GetChild(0).gameObject);

                    if (totalFruitPrice > 0)
                    {
                        totalFruitPrice /= 2;
                    }
                    else
                    {
                        totalFruitPrice = 0;
                    }
                }
            }

            // 모든 과일을 맞췄는지 확인
            if (currentFruits.Count == 0)
            {
                InputManager.Instance.BlockInput(true);

                // Fever 슬라이더가 꽉 찼는지 확인
                if (fever.IsFeverGaugeFull())
                {
                    fever.StartFeverMode();
                }

                if (MiddleBossStageManager.Instance.isMiddleBossStage == true)
                {
                    OnAllFruitsMatchedForMiddleBossStage();
                }
                else
                {
                    OnAllFruitsMatched();
                }
            }
        }
    }

    private void FruitTouchSFX()
    {
        int randomIndex = Random.Range(1, 6);
        string sfxName = $"Fruit{randomIndex}";
        SoundManager.Instance.PlaySFX(sfxName);
    }

    // 모든 과일을 맞췄을 때 호출될 메서드
    private void OnAllFruitsMatched()
    {
        Debug.Log("모든 과일을 맞췄습니다. 손님이 나갑니다.");

        // 맞춘 과일에 대한 골드 보상
        CustomerManager.Instance.RewardGoldForMatches(totalFruitPrice);

        // 금바른탕후루 업그레이드 적용 시 추가 골드 보상
        UpgradeData goldTangHuluUpgrade = null; //UpgradeManager.Instance.GetUpgradeByName("금바른탕후루");
        if (goldTangHuluUpgrade != null)
        {
            if (fever.isFeverModeActive)
            {
                GoldManager.Instance.IncreaseGold(UpgradeManager.Instance.goldTanghuluBonus * 2);
            }
            else
            {
                GoldManager.Instance.IncreaseGold(UpgradeManager.Instance.goldTanghuluBonus);
            }
        }

        // 만족도 표현
        CustomerManager.Instance.CustomerSatisfaction();

        // 손님이 올 때마다 초기화 (과일을 모두 맞춘 후)
        CustomerManager.Instance.AddGuest();

        // 손님 나가기 - 메서드 체이닝
        // CustomerManager.Instance로부터 싱글톤 인스턴스를 얻고, GetCurrentCustomer() 메서드를 호출하여
        // 현재 손님(Customer) 객체를 반환한 뒤, 그 손님 객체에서 MoveOut() 메서드를 호출
        CustomerManager.Instance.GetCurrentCustomer().MoveOut();


        // 새로운 과일 초기화
        InitializeTangHulu();
        HideFruitToMatch();
    }

    private void OnAllFruitsMatchedForMiddleBossStage()
    {
        MiddleBossStageManager.Instance.AddDeliverdTanghuluCount();
        MiddleBossStageManager.Instance.UpdateMiddleBossStageUI();

        // InputManager를 통해 입력 차단
        InputManager.Instance.BlockInput(true);

        StartCoroutine(HandleTanghuluCreationWithDelay(0.5f));
       
        // InputManager를 통해 입력 재활성화
        InputManager.Instance.BlockInput(false);
    }

    private IEnumerator HandleTanghuluCreationWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 현재 완성된 Tanghulu를 복사하여 컨테이너에 생성
        GameObject completedTanghulu = Instantiate(Tanghulu.Instance.GetCompletedTanghulu());

        if (completedTanghulu != null)
        {
            Debug.Log("Tanghulu가 성공적으로 생성되었습니다.");

            // 컨테이너의 자식으로 설정 (월드 좌표 유지)
            completedTanghulu.transform.SetParent(MiddleBossStageManager.Instance.GetTanghuluContainerPosition(), true);

            // Tanghulu의 위치를 계산하여 피라미드 형태로 배치
            Vector3 newPosition = MiddleBossStageManager.Instance.CalculateTanghuluPosition();
            completedTanghulu.transform.localPosition = newPosition;

            // 크기 조정 (필요에 따라 조정)
            completedTanghulu.transform.localScale = new Vector3(3f, 1.5f, 1f); // 크기를 적절히 조정

            // Collider2D 및 Rigidbody2D 제거
            MiddleBossStageManager.Instance.RemoveAllCollidersAndRigidbodies(completedTanghulu.transform);

            // 새로운 Tanghulu에 레이어 순서 설정
            MiddleBossStageManager.Instance.ApplyLayerOrder(completedTanghulu);

            // Tanghulu 위치 상태 업데이트
            MiddleBossStageManager.Instance.UpdateTanghuluPositionState();
        }

        // Tanghulu를 클리어하여 새로운 탕후루 준비
        Tanghulu.Instance.ClearTanghulu();
        InitializeTangHulu();
    }

    public void ShowFruitToMatch()
    {
        matchFruitPanel.SetActive(true);
    }

    public void HideFruitToMatch()
    {
        matchFruitPanel.SetActive(false);
    }
}