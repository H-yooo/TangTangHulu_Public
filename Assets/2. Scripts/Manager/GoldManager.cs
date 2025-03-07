using TMPro;
using UnityEngine;

public class GoldManager : GenericSingleton<GoldManager>
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private int _gold;

    public int Gold
    {
        get { return _gold; }
        private set
        {
            _gold = value;
            UpdateGoldUI();
        }
    }

    public void IncreaseGold(int amount)
    {
        Gold += amount;
    }

    public void DecreaseGold(int amount)
    {
        Gold = Mathf.Max(0, Gold - amount);  // 금액 감소 (0 이하로 떨어지지 않게)
    }

    public void SetGold(int amount)
    {
        Gold = amount;
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        goldText.text = "소지금 : " + Gold + "G";
        VillageManager.Instance.villageGoldText.text = "소지금 : " + Gold + "G";
    }
}