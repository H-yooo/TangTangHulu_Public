using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : GenericSingleton<LevelManager>
{
    public int currentLevel = 1;
    public int currentExp = 0;  // 현재 경험치
    public int expToNextLevel = 100;  // 다음 레벨로 가기 위한 경험치 (처음엔 100)

    public int expGainPercentage = 0; // 경험치 획득량

    [SerializeField] private Slider expBar;  // 경험치 바 (ResultPanel에 표시)
    [SerializeField] private TextMeshProUGUI levelText;  // 레벨 텍스트 (ResultPanel에 표시)
    [SerializeField] private Slider expBar_UpgradePanel;
    [SerializeField] private TextMeshProUGUI levelText_UpgradePanel;

    // 경험치 추가 메서드 (즉시 적용되지 않고 애니메이션으로 적용)
    public void AddExperienceWithAnimation(int dayFavorability)
    {
        int targetExp = currentExp + dayFavorability;  // 목표 경험치 설정
        StartCoroutine(AnimateExpIncrease(targetExp));  // 코루틴으로 경험치 애니메이션 실행
    }

    // 경험치 바 애니메이션
    private IEnumerator AnimateExpIncrease(int targetExp)
    {
        while (currentExp < targetExp)
        {
            // 경험치 증가
            currentExp += 1;  // 경험치 증가 속도 조정

            // 경험치 바 업데이트
            UpdateExpBar();

            // 경험치가 expToNextLevel에 도달하거나 초과한 경우
            if (currentExp >= expToNextLevel)
            {
                // 초과된 경험치 계산
                int remainingExp = targetExp - expToNextLevel;
                Debug.Log("레벨업! 초과 경험치: " + remainingExp);

                LevelUp();  // 레벨업

                // 초과된 경험치가 있을 경우, 초과된 경험치를 반영하여 다시 애니메이션 진행
                if (remainingExp > 0)
                {
                    currentExp = 0;  // 경험치 초기화
                    targetExp = remainingExp;  // 남은 경험치가 새로운 목표 경험치가 됨
                }
                else
                {
                    yield break;  // 초과 경험치가 없으면 코루틴 종료
                }
            }

            yield return new WaitForSeconds(0.02f);
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp = 0;  // 경험치 초기화 (초과된 경험치는 애니메이션에서 처리됨)
        expToNextLevel *= 2;  // 다음 레벨로 가기 위한 경험치 증가

        // 레벨 업 UI 업데이트
        UpdateExpBar();

    }

    // 경험치 바 업데이트
    private void UpdateExpBar()
    {
        if (expBar != null)
        {
            expBar.maxValue = expToNextLevel;
            expBar.value = currentExp;
        }

        if (expBar_UpgradePanel != null)
        {
            expBar_UpgradePanel.maxValue = expToNextLevel;
            expBar_UpgradePanel.value = currentExp;
        }

        if (levelText != null)
        {
            levelText.text = "Level " + currentLevel;
        }

        if (levelText_UpgradePanel != null)
        {
            levelText_UpgradePanel.text = "Level " + currentLevel;
        }
    }

    public void AddExperience(int expToAdd)
    {
        int targetExp = currentExp + expToAdd;
        StartCoroutine(AnimateExpIncrease(targetExp));
    }

    // ResultPanel이 활성화될 때 호출 (라운드 종료 시)
    public void EndRoundWithAnimation()
    {
        int dayFavorability = Mathf.CeilToInt(CustomerManager.Instance.dayFavorability + (CustomerManager.Instance.dayFavorability * expGainPercentage * 0.01f));  // CustomerManager에서 호감도 가져오기
        AddExperienceWithAnimation(dayFavorability);  // 경험치 애니메이션 추가
        Debug.Log("경험치 추가됨: " + dayFavorability);
    }
}