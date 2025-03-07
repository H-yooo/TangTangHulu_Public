using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "UpgradeDataSO/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public string description;
    public Sprite upgradeImage;
    public int currentRate;
    public int increaseRate;
    public int gold;
    public int level = 0;
    public int maxLevel;

    // 업그레이드 상태 초기화
    public void ResetUpgradeLevel()
    {
        currentRate = 0;
        level = 0;
    }

    // 업그레이드 효과 적용
    public void ApplyUpgrade()
    {
        switch (upgradeName)
        {
            // 1페이지
            case "연장영업":
                AddRoundTime(1);
                break;
            case "전단지돌리기":
                IncreaseExpGain(10);
                break;
            case "금바른탕후루":
                GoldTanghuluUpgrade();
                break;



            case "과일조각모음":
                IncreaseTanghuluMaxFruitCount();
                break;




            // 2페이지
            case "시식코너":
                IncreaseExpGain(10);
                break;

            




          
          

           

          
            default:
                Debug.LogWarning("알 수 없는 업그레이드: " + upgradeName);
                break;
        }
    }

    // 라운드 시간 증가
    private void AddRoundTime(int time)
    {
        TimeManager.Instance.IncreaseRoundTime(time);
        currentRate += increaseRate;
        level++;
        NotificationCanvas.Instance.ShowNotificationPanel1($"라운드 시간이 {time}초 증가합니다. \n라운드 시간 : {TimeManager.Instance.GetCurrentTime()}");
    }

    // 경험치 획득량 증가
    private void IncreaseExpGain(int increasePercentage)
    {
        currentRate += increaseRate;
        level++;
        LevelManager.Instance.expGainPercentage += increasePercentage;
        NotificationCanvas.Instance.ShowNotificationPanel1($"경험치 획득량이 {increasePercentage}% 증가했습니다. \n경험치 추가 획득량 : {LevelManager.Instance.expGainPercentage}%");
    }

    // 금바른 탕후루
    public void GoldTanghuluUpgrade()
    {
        currentRate += increaseRate;
        level++;
        int goldBonus = currentRate * 20;
        UpgradeManager.Instance.goldTanghuluBonus = goldBonus;
        NotificationCanvas.Instance.ShowNotificationPanel1($"탕후루를 만들때마다 {goldBonus} 골드가 추가됩니다.");
    }

    // Tanghulu의 최대 과일 개수를 증가시키는 메서드
    private void IncreaseTanghuluMaxFruitCount()
    {
        level++;
        Tanghulu.Instance.MaxFruitCount += 1; // MaxFruitCount를 1 증가
        NotificationCanvas.Instance.ShowNotificationPanel1($"탕후루 꼬치에 꽂을 수 있는 과일의 수가 {Tanghulu.Instance.MaxFruitCount}개로 증가했습니다.");
    }
}