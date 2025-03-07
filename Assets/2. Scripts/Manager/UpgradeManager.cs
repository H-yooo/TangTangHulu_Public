using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : GenericSingleton<UpgradeManager>
{
    [SerializeField] private UpgradeData[] initialUpgradeList; // 초기 ScriptableObject 업그레이드 목록
    public UpgradeData[] availableUpgrades;

    [Header("UI References")]
    [SerializeField] private GameObject[] Page1_Buttons; // 업그레이드 버튼들
    [SerializeField] private Image upgradeImage_Information;
    [SerializeField] private TextMeshProUGUI detailText_Information; // Lv 변화, 증가량, 비용
    [SerializeField] private TextMeshProUGUI descriptionText_Information; // 업그레이드 설명

    private const int upgradesPerPage = 9; // 한 페이지에 표시할 업그레이드 수
    //[SerializeField] private int currentPage = 0;

    [Header("Upgrade Effects")]
    public int goldTanghuluBonus;

    private void Start()
    {
        ResetUpgrades();
        UpdateCurrentShop();
    }

    public void UpdateCurrentShop()
    {
        // 디폴트로 표시할 업그레이드
        UpgradeData defaultUpgrade = FindFirstAvailableUpgrade();

        if (defaultUpgrade != null)
        {
            ShowInformation(defaultUpgrade);
        }
        else
        {
            upgradeImage_Information.sprite = availableUpgrades[0].upgradeImage; ; // 이미지 제거
            detailText_Information.text = " ";
            descriptionText_Information.text = "모든 업그레이드가 완료되었습니다!";
        }
    }

    private UpgradeData FindFirstAvailableUpgrade()
    {
        foreach (UpgradeData upgrade in availableUpgrades)
        {
            if (upgrade.level < upgrade.maxLevel) // 아직 최대 레벨이 아닌 업그레이드 찾기
            {
                return upgrade;
            }
        }
        return null; // 모든 업그레이드가 최대 레벨이라면 null 반환
    }

    // 모든 업그레이드 초기화하고, availableUpgrades 배열에 넣는다.
    public void ResetUpgrades()
    {
        foreach (var upgrade in initialUpgradeList)
        {
            if (upgrade != null)
            {
                upgrade.ResetUpgradeLevel(); // 각 업그레이드의 레벨을 초기화
            }
        }

        availableUpgrades = (UpgradeData[])initialUpgradeList.Clone();
        AssignUpgradesToButtons();
    }

    // 업그레이드를 버튼에 할당
    private void AssignUpgradesToButtons()
    {
        for (int i = 0; i < Page1_Buttons.Length; i++)
        {
            UpgradeData upgradeData = availableUpgrades[i];
            SetButtonData(Page1_Buttons[i], upgradeData);
        }
    }

    // 버튼의 데이터를 채우는 함수
    private void SetButtonData(GameObject buttonObj, UpgradeData upgradeData)
    {
        ShopUpgradeButton shopUpgradeButton = buttonObj.GetComponent<ShopUpgradeButton>();

        if (shopUpgradeButton != null)
        {
            shopUpgradeButton.SetData(upgradeData);

            // 메인 버튼 클릭 시 PurchaseBtn 활성화
            Button mainButton = buttonObj.GetComponent<Button>();
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() =>
            {
                HidePurchaseButtons(); // 다른 버튼들의 구매 버튼 숨기기
                ShowInformation(upgradeData); // 레벨과 효과 정보를 표시

                // 구매 버튼 표시 여부 결정
                if (upgradeData.level >= upgradeData.maxLevel)
                {
                    shopUpgradeButton.HidePurchaseButton(); // 최대 레벨이면 구매 버튼 숨김
                }
                else
                {
                    shopUpgradeButton.ShowPurchaseButton(); // 최대 레벨이 아니면 구매 버튼 표시
                }
            });
        }
    }

    private void HidePurchaseButtons()
    {
        foreach (var button in Page1_Buttons)
        {
            ShopUpgradeButton shopUpgradeButton = button.GetComponent<ShopUpgradeButton>();
            if (shopUpgradeButton != null)
            {
                shopUpgradeButton.HidePurchaseButton();
            }
        }
    }

    // Information 패널에 데이터 표시
    public void ShowInformation(UpgradeData upgradeData)
    {
        if (upgradeData != null)
        {
            // 이미지 업데이트
            upgradeImage_Information.sprite = upgradeData.upgradeImage;

            // 최대 레벨 여부 확인
            bool isMaxLevel = upgradeData.level >= upgradeData.maxLevel;

            if (isMaxLevel)
            {
                // 최대 레벨에 도달한 경우 텍스트 변경
                detailText_Information.text = $"Lv. {upgradeData.level} (Max)\n" +
                                  $"증가량: {upgradeData.currentRate} (Max)";
                descriptionText_Information.text = upgradeData.description;

                // 구매 버튼 비활성화
                HidePurchaseButtons();
            }
            else
            {
                // 최대 레벨이 아닌 경우 기존 로직 유지
                bool canAfford = GoldManager.Instance.Gold >= upgradeData.gold;
                string colorTag = canAfford ? "<color=#000000>" : "<color=#FF0000>"; // 검정색 또는 빨간색
                string endColorTag = "</color>";

                detailText_Information.text = $"Lv. {upgradeData.level} -> Lv.{upgradeData.level + 1} (Max: {upgradeData.maxLevel})\n" +
                                  $"증가량: {upgradeData.currentRate} -> {upgradeData.currentRate + upgradeData.increaseRate}\n" +
                                  $"비용: {colorTag}{upgradeData.gold}G{endColorTag}";
                descriptionText_Information.text = upgradeData.description;
            }
        }
    }

    public void RoadSavedUpgradeData()
    {
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade.level > 0) // 업그레이드 레벨이 0보다 크면 재적용
            {
                ApplyUpgradeEffect(upgrade);
            }
        }

        Debug.Log("모든 업그레이드가 재적용되었습니다.");
    }

    private void ApplyUpgradeEffect(UpgradeData upgrade)
    {
        switch (upgrade.upgradeName)
        {
            case "연장영업":
                TimeManager.Instance.IncreaseRoundTime(upgrade.currentRate);
                break;
            case "전단지돌리기":
                LevelManager.Instance.expGainPercentage += upgrade.currentRate;
                break;
            case "금바른탕후루":
                Instance.goldTanghuluBonus = upgrade.currentRate * 20;
                break;
            case "과일조각모음":
                Tanghulu.Instance.MaxFruitCount += upgrade.level;
                break;
            default:
                Debug.LogWarning($"알 수 없는 업그레이드: {upgrade.upgradeName}");
                break;
        }
    }
}