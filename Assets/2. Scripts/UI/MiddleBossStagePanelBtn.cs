using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiddleBossStagePanelBtn : MonoBehaviour
{
    [SerializeField] private Button[] middleBossStageButtons;
    [SerializeField] private TextMeshProUGUI[] unlockConditionTexts;
    [SerializeField] private int[] unlockConditions = { 10, 30, 50, 70, 90 };
    [SerializeField] private Button extraRewardBtn;
    [SerializeField] private Button openVillageBtn;

    private void Start()
    {
        extraRewardBtn.onClick.AddListener(OnExtraRewardBtnClicked);
        openVillageBtn.onClick.AddListener(OnVillageBtnClicked);

        SetupMiddleBossStageButtons();
    }

    public void SetupMiddleBossStageButtons()
    {
        int currentDay = StageManager.Instance.currentStageLevel;

        for (int i = 0; i < middleBossStageButtons.Length; i++)
        {
            Button stageButton = middleBossStageButtons[i];
            TextMeshProUGUI conditionText = unlockConditionTexts[i];

            if (currentDay >= unlockConditions[i])
            {   
                stageButton.interactable = true;
                conditionText.gameObject.SetActive(false); // "해금조건" 텍스트 비활성화

                stageButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
                int index = i; // 로컬 변수로 캡처
                stageButton.onClick.AddListener(() => MiddleBossStageManager.Instance.OnMiddleBossStageButtonClicked(index));
            }
            else
            {
                stageButton.interactable = false;

                // 버튼 알파값 조정 (50% 투명)
                ColorBlock colors = stageButton.colors;
                colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.5f);
                stageButton.colors = colors;

                conditionText.gameObject.SetActive(true); // "해금조건" 텍스트 활성화
                conditionText.text = $"해금조건: Day {unlockConditions[i]}";
            }
        }
    }

    // 추가 보상 버튼 클릭 시 동작
    private void OnExtraRewardBtnClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        extraRewardBtn.interactable = false; // 버튼 클릭 불가

        // 버튼 흐리게 처리 (alpha값 변경)
        ColorBlock colors = extraRewardBtn.colors;
        colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.5f);  // 50% 투명도
        extraRewardBtn.colors = colors;

        GameManager.Instance.ExtraRewardMiddleBossStage();
    }

    // 확인 버튼 클릭 시 동작
    private void OnVillageBtnClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        MiddleBossStageManager.Instance.EndMiddleBossStage();
        GameManager.Instance.CloseMiddleBossResultPanel();
        GameManager.Instance.SaveGameData();
        VillageManager.Instance.OpenVillage();

        ColorBlock colors = extraRewardBtn.colors;
        colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 1f);
        extraRewardBtn.interactable = true;
    }
}
