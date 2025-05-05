using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FruitUpgradeButton : MonoBehaviour
{
    [SerializeField] private Image fruitImage;          // ���� �̹���
    [SerializeField] private TextMeshProUGUI descriptionText; // ���� ����
    [SerializeField] private Button upgradeButton;      // ���׷��̵� ��ư
    [SerializeField] private TextMeshProUGUI costText;        // ��� �ؽ�Ʈ

    private FruitData fruitData; // ���� ���� ������

    public void Start()
    {
        CheckUpgradeAvailability();
    }

    public void SetData(FruitData data)
    {
        fruitData = data;

        // UI ��ҿ� ������ ����
        fruitImage.sprite = data.fruitImage.sprite;
        UpgradeInformationWithoutSound();

        // ��ư �̺�Ʈ ����
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
    }

    private void OnUpgradeButtonClicked()
    {
        if (fruitData.fruitLevel < 10) // ���� 10 �̸��� ���� ������ ����
        {
            if (GoldManager.Instance.Gold >= fruitData.fruitPrice)
            {
                GoldManager.Instance.DecreaseGold(fruitData.upgradeCost);
                fruitData.fruitLevel++;
                fruitData.fruitPrice += fruitData.increaseRate;

                // UI ����
                UpgradeInformation();
            }
            else
            {
                NotificationCanvas.Instance.ShowNotificationPanel1("��尡 �����մϴ�!");
            }
        }
        else
        {
            UpgradeToNextFruit();
        }
    }

    private void UpgradeInformationWithoutSound()
    {
        if (fruitData.fruitLevel < 10)
        {
            descriptionText.text = $"{fruitData.fruitName} \nLevel : Lv.{fruitData.fruitLevel} -> Lv.{fruitData.fruitLevel + 1} \nPrice : {fruitData.fruitPrice}G -> {fruitData.fruitPrice + fruitData.increaseRate}G";
            costText.text = $"{fruitData.upgradeCost}G";
        }
        else
        {
            descriptionText.text = "Ư�� Ʈ���Ǹ� ȹ���ؾ� ����� Up�� �� �ֽ��ϴ�.";
            costText.text = GetTournamentStage();
            CheckUpgradeAvailability();
        }
    }

    private void UpgradeInformation()
    {
        if (fruitData.fruitLevel < 10)
        {
            SoundManager.Instance.PlaySFX("Purchase");
            descriptionText.text = $"{fruitData.fruitName} \nLevel : Lv.{fruitData.fruitLevel} -> Lv.{fruitData.fruitLevel + 1} \nPrice : {fruitData.fruitPrice}G -> {fruitData.fruitPrice + fruitData.increaseRate}G";
            costText.text = $"{fruitData.upgradeCost}G";
        }
        else
        {
            descriptionText.text = "Ư�� Ʈ���Ǹ� ȹ���ؾ� ����� Up�� �� �ֽ��ϴ�.";
            costText.text = GetTournamentStage();
            CheckUpgradeAvailability();
        }
    }

    public void CheckUpgradeAvailability()
    {
        int index = FruitManager.Instance.EntireFruitDataList.IndexOf(fruitData);
        bool canUpgrade = false;

        if (index >= 0 && index <= 4)
        {
            canUpgrade = GameManager.Instance.IsBossStageCleared(0);
        }
        else if (index >= 5 && index <= 9)
        {
            canUpgrade = GameManager.Instance.IsBossStageCleared(1);
        }
        else if (index >= 10 && index <= 14)
        {
            canUpgrade = GameManager.Instance.IsBossStageCleared(2);
        }
        else if (index >= 15 && index <= 19)
        {
            canUpgrade = GameManager.Instance.IsBossStageCleared(3);
        }
        else
        {
            canUpgrade = false;
        }

        if (!canUpgrade)
        {
            ColorBlock colors = upgradeButton.colors;
            colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.5f);
            upgradeButton.colors = colors;
            upgradeButton.interactable = false;
        }
        else
        {
            upgradeButton.interactable = true; // Ŭ���� �� �ٽ� Ȱ��ȭ
            canUpgrade = false;
        }
    }

    private string GetTournamentStage()
    {
        int index = FruitManager.Instance.EntireFruitDataList.IndexOf(fruitData);

        if (index >= 0 && index <= 4)
            return "������ȸ";
        else if (index >= 5 && index <= 9)
            return "������ȸ";
        else if (index >= 10 && index <= 14)
            return "�����ȸ";
        else if (index >= 15 && index <= 19)
            return "���ִ�ȸ";
        else
            return "Max";
    }

    private void UpgradeToNextFruit()
    {
        int nextIndex = fruitData.fruitIndex + 5; // ���� ������ index ���

        // EntireFruitDataList���� ���� ���� �����͸� ��������
        if (nextIndex < FruitManager.Instance.EntireFruitDataList.Count)
        {
            FruitData nextFruitData = FruitManager.Instance.EntireFruitDataList.Find(data => data.fruitIndex == nextIndex);

            if (nextFruitData != null)
            {
                // ���� ���� ������ �ʱ�ȭ
                nextFruitData.fruitLevel = 1; // �� ���� ���� �ʱ�ȭ
                nextFruitData.fruitPrice = nextFruitData.initialFruitPrice; // ���� �ʱ�ȭ

                // CurrentFruitDataList ������Ʈ
                FruitManager.Instance.UpdateCurrentFruitDataList(fruitData.fruitIndex, nextFruitData);

                // UI ����
                SetData(nextFruitData);

                NotificationCanvas.Instance.ShowNotificationPanel1($"�����մϴ�! {nextFruitData.fruitName}(��)�� ���׷��̵� �Ǿ����ϴ�.");
            }
            else
            {
                NotificationCanvas.Instance.ShowNotificationPanel1("���� ���� �����͸� ã�� �� �����ϴ�!");
            }
        }
        else
        {
            NotificationCanvas.Instance.ShowNotificationPanel1("�� �̻� ���׷��̵� �� �� �ִ� ������ �����ϴ�!");
        }
    }
}
