using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FruitUpgradeButton : MonoBehaviour
{
    [SerializeField] private Image fruitImage;          // 과일 이미지
    [SerializeField] private TextMeshProUGUI descriptionText; // 과일 설명
    [SerializeField] private Button upgradeButton;      // 업그레이드 버튼
    [SerializeField] private TextMeshProUGUI costText;        // 비용 텍스트

    private FruitData fruitData; // 현재 과일 데이터

    public void Start()
    {
        CheckUpgradeAvailability();
    }

    public void SetData(FruitData data)
    {
        fruitData = data;

        // UI 요소에 데이터 설정
        fruitImage.sprite = data.fruitImage.sprite;
        UpgradeInformationWithoutSound();

        // 버튼 이벤트 설정
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
    }

    private void OnUpgradeButtonClicked()
    {
        if (fruitData.fruitLevel < 10) // 레벨 10 미만일 때는 레벨만 증가
        {
            if (GoldManager.Instance.Gold >= fruitData.fruitPrice)
            {
                GoldManager.Instance.DecreaseGold(fruitData.upgradeCost);
                fruitData.fruitLevel++;
                fruitData.fruitPrice += fruitData.increaseRate;

                // UI 갱신
                UpgradeInformation();
            }
            else
            {
                NotificationCanvas.Instance.ShowNotificationPanel1("골드가 부족합니다!");
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
            descriptionText.text = "특정 트로피를 획득해야 등급을 Up할 수 있습니다.";
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
            descriptionText.text = "특정 트로피를 획득해야 등급을 Up할 수 있습니다.";
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
            upgradeButton.interactable = true; // 클리어 시 다시 활성화
            canUpgrade = false;
        }
    }

    private string GetTournamentStage()
    {
        int index = FruitManager.Instance.EntireFruitDataList.IndexOf(fruitData);

        if (index >= 0 && index <= 4)
            return "지역대회";
        else if (index >= 5 && index <= 9)
            return "전국대회";
        else if (index >= 10 && index <= 14)
            return "세계대회";
        else if (index >= 15 && index <= 19)
            return "우주대회";
        else
            return "Max";
    }

    private void UpgradeToNextFruit()
    {
        int nextIndex = fruitData.fruitIndex + 5; // 다음 과일의 index 계산

        // EntireFruitDataList에서 다음 과일 데이터를 가져오기
        if (nextIndex < FruitManager.Instance.EntireFruitDataList.Count)
        {
            FruitData nextFruitData = FruitManager.Instance.EntireFruitDataList.Find(data => data.fruitIndex == nextIndex);

            if (nextFruitData != null)
            {
                // 다음 과일 데이터 초기화
                nextFruitData.fruitLevel = 1; // 새 과일 레벨 초기화
                nextFruitData.fruitPrice = nextFruitData.initialFruitPrice; // 가격 초기화

                // CurrentFruitDataList 업데이트
                FruitManager.Instance.UpdateCurrentFruitDataList(fruitData.fruitIndex, nextFruitData);

                // UI 갱신
                SetData(nextFruitData);

                NotificationCanvas.Instance.ShowNotificationPanel1($"축하합니다! {nextFruitData.fruitName}(으)로 업그레이드 되었습니다.");
            }
            else
            {
                NotificationCanvas.Instance.ShowNotificationPanel1("다음 과일 데이터를 찾을 수 없습니다!");
            }
        }
        else
        {
            NotificationCanvas.Instance.ShowNotificationPanel1("더 이상 업그레이드 할 수 있는 과일이 없습니다!");
        }
    }
}
