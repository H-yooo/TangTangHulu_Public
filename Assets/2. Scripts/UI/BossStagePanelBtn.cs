using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossStagePanelBtn : MonoBehaviour
{
    [SerializeField] private Button[] bossStageButtons; // 보스 스테이지 버튼 배열
    [SerializeField] private TextMeshProUGUI[] unlockConditionTexts; // "해금조건" 텍스트 배열
    private int[] unlockConditions = { 20, 40, 60, 80, 100 }; // 보스 스테이지 해금 조건
    [SerializeField] private Button openVillageBtn;

    private int selectedBossStageIndex = -1;

    private void Start()
    {
        openVillageBtn.onClick.AddListener(OnVillageBtnClicked);
        SetupBossStageButtons();
    }

    public void SetupBossStageButtons()
    {
        int currentDay = StageManager.Instance.currentStageLevel;
        string[] trophyTexts = { "지역대회 트로피 획득", "전국대회 트로피 획득", "세계대회 트로피 획득", "우주대회 트로피 획득", "심연대회 트로피 획득" };

        for (int i = 0; i < bossStageButtons.Length; i++)
        {
            Button stageButton = bossStageButtons[i];
            TextMeshProUGUI conditionText = unlockConditionTexts[i];

            if (currentDay >= unlockConditions[i])
            {
                stageButton.interactable = true; // 해금 조건 만족 시 활성화
                stageButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
                int index = i; // 로컬 변수로 캡처
                stageButton.onClick.AddListener(() => OnBossStageButtonClicked(index));

                if (GameManager.Instance.IsBossStageCleared(i)) // "해금조건" 텍스트 변경
                {
                    conditionText.color = Color.green;
                    conditionText.text = trophyTexts[i];
                    conditionText.gameObject.SetActive(true);
                }
                else
                {
                    conditionText.gameObject.SetActive(false);
                }
            }
            else
            {
                stageButton.interactable = false; // 조건 미충족 시 비활성화

                // 버튼 알파값 조정 (50% 투명)
                ColorBlock colors = stageButton.colors;
                colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.5f);
                stageButton.colors = colors;

                conditionText.gameObject.SetActive(true); // "해금조건" 텍스트 활성화
                conditionText.text = $"해금조건: Day {unlockConditions[i]}";
            }
        }
    }

    private void OnBossStageButtonClicked(int index)
    {
        selectedBossStageIndex = index; // 클릭한 스테이지 값 저장
        BossStageManager.Instance.StartBossStage(index);
    }

    public int GetSelectedBossStageIndex()
    {
        return selectedBossStageIndex;
    }

    // 확인 버튼 클릭 시 동작
    private void OnVillageBtnClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        BossStageManager.Instance.EndBossStage();
        GameManager.Instance.CloseMiddleBossResultPanel();
        GameManager.Instance.SaveGameData();
        VillageManager.Instance.OpenVillage();
    }
}