using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUpgradeButton : MonoBehaviour
{
    [SerializeField] private Image upgradeImage;
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeLevelText;
    [SerializeField] private Button purchaseButton;

    private UpgradeData currentUpgradeData; // 현재 업그레이드 데이터

    // 업그레이드 데이터 적용
    public void SetData(UpgradeData upgradeData)
    {
        currentUpgradeData = upgradeData;

        upgradeImage.sprite = upgradeData.upgradeImage;
        upgradeNameText.text = upgradeData.upgradeName;
        upgradeLevelText.text = $"Level: {upgradeData.level}";

        purchaseButton.gameObject.SetActive(false); // 구매 버튼 비활성화
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
    }

    // 업그레이드 UI 갱신
    public void UpdateData(UpgradeData upgradeData)
    {
        upgradeNameText.text = upgradeData.upgradeName;
        upgradeLevelText.text = $"Level: {upgradeData.level}";
    }

    // 구매 버튼 클릭 시 실행
    private void OnPurchaseButtonClicked()
    {
        if (GoldManager.Instance.Gold >= currentUpgradeData.gold)
        {
            SoundManager.Instance.PlaySFX("Purchase");

            GoldManager.Instance.DecreaseGold(currentUpgradeData.gold);
            upgradeLevelText.text = $"Level: {currentUpgradeData.level}";

            // 구매 후 PurchaseBtn 비활성화
            purchaseButton.gameObject.SetActive(false);

            // 추가적인 업그레이드 효과 적용
            currentUpgradeData.ApplyUpgrade();
            UpdateData(currentUpgradeData);
            UpgradeManager.Instance.ShowInformation(currentUpgradeData);

            NotificationCanvas.Instance.ShowNotificationPanel1($"{currentUpgradeData.upgradeName} 업그레이드 완료! 현재 레벨: {currentUpgradeData.level}");
        }
        else
        {
            NotificationCanvas.Instance.ShowNotificationPanel1("골드가 부족합니다!");
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