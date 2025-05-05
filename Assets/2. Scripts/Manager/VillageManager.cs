using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageManager : GenericSingleton<VillageManager>
{
    [SerializeField] private GameObject village;
    public TextMeshProUGUI villageStageText;
    public TextMeshProUGUI villageGoldText;


    [SerializeField] private Button middleBossBtn;
    [SerializeField] private Button bossBtn;
    [SerializeField] private Button fruitUpgradeBtn;
    [SerializeField] private Button shopUpgradeBtn;
    [SerializeField] private Button branchBtn;
    [SerializeField] private Button openShopBtn;

    [SerializeField] private GameObject middleBossPanel;
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private GameObject fruitUpgradePanel;
    [SerializeField] private GameObject shopUpgradePanel;
    [SerializeField] private GameObject branchPanel;
    [SerializeField] private GameObject openShop;

    private bool isMiddleBossPanelOpen = false;
    private bool isBossPanelOpen = false;
    private bool isFruitUpgradePanelOpen = false;
    private bool isShopPanelOpen = false;
    private bool isBranchPanelOpen = false;

    private void Start()
    {
        middleBossBtn.onClick.AddListener(OnMiddleBossPanelOpen);
        bossBtn.onClick.AddListener(OnBossPanelOpen);
        fruitUpgradeBtn.onClick.AddListener(OnFruitUpgradePanelOpen);
        shopUpgradeBtn.onClick.AddListener(OnShopUpgradePanelOpen);
        branchBtn.onClick.AddListener(OnBranchPanelOpen);
        openShopBtn.onClick.AddListener(OnOpenShop);
    }

    private void OnMiddleBossPanelOpen()
    {
        SoundManager.Instance.PlaySFX("Click");
        isMiddleBossPanelOpen = true;
        middleBossPanel.SetActive(true);
        openShop.SetActive(false);
    }

    private void OnBossPanelOpen()
    {
        SoundManager.Instance.PlaySFX("Click");
        isBossPanelOpen = true;
        bossPanel.SetActive(true);
        openShop.SetActive(false);
    }

    private void OnFruitUpgradePanelOpen()
    {
        SoundManager.Instance.PlaySFX("Click");
        isFruitUpgradePanelOpen = true;
        fruitUpgradePanel.SetActive(true);
        openShop.SetActive(false);
    }

    private void OnShopUpgradePanelOpen()
    {
        SoundManager.Instance.PlaySFX("Click");
        isShopPanelOpen = true;
        shopUpgradePanel.SetActive(true);
        openShop.SetActive(false);
    }

    private void OnBranchPanelOpen()
    {
        SoundManager.Instance.PlaySFX("Click");
        NotificationCanvas.Instance.ShowNotificationPanel1("추후 업데이트 예정입니다!");
        //isBranchPanelOpen = true;
        //branchPanel.SetActive(true);
        //openShop.SetActive(false);
    }

    public void OnOpenShop()
    {
        ClosePanelBtn();
        CloseVillage();
        GameManager.Instance.StartNextRound();
    }

    public void ClosePanelBtn()
    {
        SoundManager.Instance.PlaySFX("Click");
        if (isMiddleBossPanelOpen)
        {
            middleBossPanel.SetActive(false);
            isMiddleBossPanelOpen = false;
        }
        else if (isBossPanelOpen)
        {
            bossPanel.SetActive(false);
            isBossPanelOpen = false;
        }
        else if (isFruitUpgradePanelOpen)
        {
            fruitUpgradePanel.SetActive(false);
            isFruitUpgradePanelOpen = false;
        }
        else if (isShopPanelOpen)
        {
            shopUpgradePanel.SetActive(false);
            isShopPanelOpen = false;
        }
        else if (isBranchPanelOpen)
        {
            branchPanel.SetActive(false);
            isBranchPanelOpen = false;
        }
        else
        {
            return;
        }

        openShop.SetActive(true);
    }

    public void OpenVillage()
    {
        StageManager.Instance.UpdateStageUI();
        GoldManager.Instance.UpdateGoldUI();
        village.SetActive(true);
        SoundManager.Instance.PlayBGM("Village", 0f);
    }

    public void CloseVillage()
    {
        village.SetActive(false);
    }
}
