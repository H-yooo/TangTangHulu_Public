using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUpgradeButton : MonoBehaviour
{
    [SerializeField] private Image upgradeImage;
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeLevelText;
    [SerializeField] private Button purchaseButton;

    private UpgradeData currentUpgradeData; // ���� ���׷��̵� ������

    // ���׷��̵� ������ ����
    public void SetData(UpgradeData upgradeData)
    {
        currentUpgradeData = upgradeData;

        upgradeImage.sprite = upgradeData.upgradeImage;
        upgradeNameText.text = upgradeData.upgradeName;
        upgradeLevelText.text = $"Level: {upgradeData.level}";

        purchaseButton.gameObject.SetActive(false); // ���� ��ư ��Ȱ��ȭ
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
    }

    // ���׷��̵� UI ����
    public void UpdateData(UpgradeData upgradeData)
    {
        upgradeNameText.text = upgradeData.upgradeName;
        upgradeLevelText.text = $"Level: {upgradeData.level}";
    }

    // ���� ��ư Ŭ�� �� ����
    private void OnPurchaseButtonClicked()
    {
        if (GoldManager.Instance.Gold >= currentUpgradeData.gold)
        {
            SoundManager.Instance.PlaySFX("Purchase");

            GoldManager.Instance.DecreaseGold(currentUpgradeData.gold);
            upgradeLevelText.text = $"Level: {currentUpgradeData.level}";

            // ���� �� PurchaseBtn ��Ȱ��ȭ
            purchaseButton.gameObject.SetActive(false);

            // �߰����� ���׷��̵� ȿ�� ����
            currentUpgradeData.ApplyUpgrade();
            UpdateData(currentUpgradeData);
            UpgradeManager.Instance.ShowInformation(currentUpgradeData);

            NotificationCanvas.Instance.ShowNotificationPanel1($"{currentUpgradeData.upgradeName} ���׷��̵� �Ϸ�! ���� ����: {currentUpgradeData.level}");
        }
        else
        {
            NotificationCanvas.Instance.ShowNotificationPanel1("��尡 �����մϴ�!");
        }
    }

    public void ShowPurchaseButton()
    {
        purchaseButton.gameObject.SetActive(true);
    }

    public void HidePurchaseButton()
    {
        purchaseButton.gameObject.SetActive(false);
    }
}