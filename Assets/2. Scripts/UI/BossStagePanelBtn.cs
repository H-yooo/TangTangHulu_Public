using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossStagePanelBtn : MonoBehaviour
{
    [SerializeField] private Button[] bossStageButtons; // ���� �������� ��ư �迭
    [SerializeField] private TextMeshProUGUI[] unlockConditionTexts; // "�ر�����" �ؽ�Ʈ �迭
    private int[] unlockConditions = { 20, 40, 60, 80, 100 }; // ���� �������� �ر� ����
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
        string[] trophyTexts = { "������ȸ Ʈ���� ȹ��", "������ȸ Ʈ���� ȹ��", "�����ȸ Ʈ���� ȹ��", "���ִ�ȸ Ʈ���� ȹ��", "�ɿ���ȸ Ʈ���� ȹ��" };

        for (int i = 0; i < bossStageButtons.Length; i++)
        {
            Button stageButton = bossStageButtons[i];
            TextMeshProUGUI conditionText = unlockConditionTexts[i];

            if (currentDay >= unlockConditions[i])
            {
                stageButton.interactable = true; // �ر� ���� ���� �� Ȱ��ȭ
                stageButton.onClick.RemoveAllListeners(); // ���� ������ ����
                int index = i; // ���� ������ ĸó
                stageButton.onClick.AddListener(() => OnBossStageButtonClicked(index));

                if (GameManager.Instance.IsBossStageCleared(i)) // "�ر�����" �ؽ�Ʈ ����
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
                stageButton.interactable = false; // ���� ������ �� ��Ȱ��ȭ

                // ��ư ���İ� ���� (50% ����)
                ColorBlock colors = stageButton.colors;
                colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.5f);
                stageButton.colors = colors;

                conditionText.gameObject.SetActive(true); // "�ر�����" �ؽ�Ʈ Ȱ��ȭ
                conditionText.text = $"�ر�����: Day {unlockConditions[i]}";
            }
        }
    }

    private void OnBossStageButtonClicked(int index)
    {
        selectedBossStageIndex = index; // Ŭ���� �������� �� ����
        BossStageManager.Instance.StartBossStage(index);
    }

    public int GetSelectedBossStageIndex()
    {
        return selectedBossStageIndex;
    }

    // Ȯ�� ��ư Ŭ�� �� ����
    private void OnVillageBtnClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        BossStageManager.Instance.EndBossStage();
        GameManager.Instance.CloseMiddleBossResultPanel();
        GameManager.Instance.SaveGameData();
        VillageManager.Instance.OpenVillage();
    }
}