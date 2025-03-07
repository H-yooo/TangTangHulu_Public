using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : GenericSingleton<GameManager>
{
    private int startingGold; // 스테이지 시작 시 소지금
    private int earnedGold;
    private int expenditureGold;
    private int additionalDeliveredGold;
    private int closingGold;
    private bool[] bossStageCleared = new bool[5]; // 보스 스테이지 1~5 클리어 여부

    public string selectedSlot;

    [SerializeField] private BossStagePanelBtn bossStagePanelBtn;
    [SerializeField] private MiddleBossStagePanelBtn middleBossStagePanelBtn;
    [SerializeField] private FruitUpgradeButton fruitUpgradeButton;
    [SerializeField] private MatchFruit matchFruit;
    [SerializeField] private Fever fever;
    [SerializeField] private LobbyCanvas lobbyCanvas;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject middleBossStageResultPanel;
    [SerializeField] private GameObject bossStageResultPanel;
    [SerializeField] private TextMeshProUGUI dayText, startingGoldText, earnedGoldText, expenditureText, closingText, storeRatingText, targetTanghuluText, middleBossEarnedGoldText, additionalDeliveredGoldText, middleBossClosingText, bossStageRankText, bossStageTanghuluCountText, bossStageEarnGoldText;

    [SerializeField] private GameObject fruitManagerObj;
    [SerializeField] private GameObject inputManagerObj;
    [SerializeField] private GameObject shutterObj;

    private void Start()
    {
        startingGold = GoldManager.Instance.Gold;
    }

    public void SetSelectedSlot(string slot)
    {
        selectedSlot = slot;
    }

    public void ShowResults()
    {
        // 셔터 다운
        InputManager.Instance.BlockInput(true);
        Shutter.Instance.DelayShutterDown(1.5f);

        // 대기 손님 초기화
        CustomerManager.Instance.ClearAllCustomers();

        if (MiddleBossStageManager.Instance.isMiddleBossStage)
        {
            StartCoroutine(DelayOpenMiddleBossResulaPenel(2.5f));

            if (MiddleBossStageManager.Instance.deliveredTanghuluCount >= MiddleBossStageManager.Instance.targetTanghuluCount)
            {
                earnedGold = MiddleBossStageManager.Instance.targetTanghuluCount * MiddleBossStageManager.Instance.targetTanghuluGold;
                additionalDeliveredGold = (MiddleBossStageManager.Instance.deliveredTanghuluCount - MiddleBossStageManager.Instance.targetTanghuluCount) * MiddleBossStageManager.Instance.additionalTanghuluGold;
                closingGold = startingGold + earnedGold + additionalDeliveredGold;
            }
            else
            {
                earnedGold = MiddleBossStageManager.Instance.deliveredTanghuluCount * MiddleBossStageManager.Instance.targetTanghuluGold;
                additionalDeliveredGold = 0;
                closingGold = startingGold + earnedGold + additionalDeliveredGold;
                MiddleBossStageManager.Instance.middleBossStageUpgradeBtn.interactable = false;

                // 버튼 흐리게 처리 (alpha값 변경)
                ColorBlock colors = MiddleBossStageManager.Instance.middleBossStageUpgradeBtn.colors;
                colors.disabledColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0.5f);  // 50% 투명도
                MiddleBossStageManager.Instance.middleBossStageUpgradeBtn.colors = colors;
            }

            targetTanghuluText.text = $"납품현황 : {MiddleBossStageManager.Instance.deliveredTanghuluCount} / {MiddleBossStageManager.Instance.targetTanghuluCount}";
            middleBossEarnedGoldText.text = "성공보수 : " + earnedGold;
            additionalDeliveredGoldText.text = "추가납품 보수 : " + additionalDeliveredGold;
            middleBossClosingText.text = "결산 : " + closingGold;
        }
        else if (BossStageManager.Instance.isBossStage)
        {
            Tanghulu.Instance.ClearTanghulu();
            StartCoroutine(DelayOpenBossResulaPenel(2.5f));

            int bossStageIndex = bossStagePanelBtn.GetSelectedBossStageIndex(); // 선택된 보스 스테이지 인덱스 가져오기
            int playerRank = BossStageManager.Instance.playerRank; // 플레이어 랭크 확인

            // 1~3등: 과일 강화 해금
            if (playerRank <= 3)
            {
                bossStageRankText.text = $"등수 : {BossStageManager.Instance.playerRank}등";
                bossStageTanghuluCountText.text = $"쌓은 개수 : {BossStageManager.Instance.playerTanghuluCount}개";
                bossStageEarnGoldText.text = "해금 : 신규 과일강화 해금";

                closingGold = startingGold;

                SetBossStageCleared(bossStageIndex);
            }
            else if (playerRank > 3)
            {
                bossStageRankText.text = $"등수 : {BossStageManager.Instance.playerRank}등";
                bossStageTanghuluCountText.text = $"쌓은 개수 : {BossStageManager.Instance.playerTanghuluCount}개";
                bossStageEarnGoldText.text = "해금 : 3등 이내로 달성 시";

                closingGold = startingGold;
            }
        }
        else
        {
            earnedGold = GoldManager.Instance.Gold - startingGold;  // 오늘 번 돈 계산
            expenditureGold = ((int)(earnedGold * 0.3f));  // 월세 및 공과금 계산 업그레이드 변수 만들기
            closingGold = startingGold + earnedGold - expenditureGold;  // 결산 계산

            // UI 텍스트에 결과 표시
            dayText.text = "Day " + StageManager.Instance.currentStageLevel + "결산";
            startingGoldText.text = "시작 금액 : " + startingGold + "G";
            earnedGoldText.text = "수입 : " + earnedGold + "G";
            expenditureText.text = "지출 : " + expenditureGold + "G";
            closingText.text = "결산 : " + closingGold + "G";
            storeRatingText.text = "가게 평점 : " + CustomerManager.Instance.GetStoreRating().ToString("F1") + "점";
            LevelManager.Instance.EndRoundWithAnimation();

            // 결과창 활성화
            StartCoroutine(DelayOpenResulaPenel(2.5f));
        }

        GoldManager.Instance.SetGold(closingGold);
        GoldManager.Instance.UpdateGoldUI();
        bossStagePanelBtn.SetupBossStageButtons();
        middleBossStagePanelBtn.SetupMiddleBossStageButtons();
        fruitUpgradeButton.CheckUpgradeAvailability();
    }

    public void ReduceExpenditure()
    {
        int currentExpenditure = expenditureGold / 2;
        expenditureGold = currentExpenditure;
        closingGold = startingGold + earnedGold - expenditureGold;  // 결산 계산

        GoldManager.Instance.SetGold(closingGold);
        GoldManager.Instance.UpdateGoldUI();

        // UI 결과 갱신
        expenditureText.text = "지출 : " + expenditureGold + "G";
        closingText.text = "결산 : " + closingGold + "G";
    }

    public void ExtraRewardMiddleBossStage()
    {
        earnedGold = Mathf.RoundToInt(earnedGold * 1.5f);
        additionalDeliveredGold = Mathf.RoundToInt(additionalDeliveredGold * 1.5f);
        closingGold = startingGold + earnedGold + additionalDeliveredGold;

        GoldManager.Instance.SetGold(closingGold);
        GoldManager.Instance.UpdateGoldUI();

        // UI 결과 갱신
        middleBossEarnedGoldText.text = "성공보수 : " + earnedGold;
        additionalDeliveredGoldText.text = "추가납품 보수 : " + additionalDeliveredGold;
        middleBossClosingText.text = "결산 : " + closingGold;
    }

    private IEnumerator DelayOpenResulaPenel(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (resultPanel == null)
        {
            resultPanel = GameObject.Find("ResultPanel");
        }

        resultPanel.SetActive(true);
    }

    private IEnumerator DelayOpenMiddleBossResulaPenel(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (middleBossStageResultPanel == null)
        {
            middleBossStageResultPanel = GameObject.Find("MiddleBossStageResultPanel");
        }

        middleBossStageResultPanel.SetActive(true);
    }

    private IEnumerator DelayOpenBossResulaPenel(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bossStageResultPanel == null)
        {
            bossStageResultPanel = GameObject.Find("BossStageResultPanel");
        }

        bossStageResultPanel.SetActive(true);
    }

    public void CloseResultPanel()
    {
        resultPanel.SetActive(false);
    }

    public void CloseMiddleBossResultPanel()
    {
        middleBossStageResultPanel.SetActive(false);
    }

    public void CloseBossResultPanel()
    {
        bossStageResultPanel.SetActive(false);
    }

    // 스테이지 별 초기화
    public void StartNextRound()
    {
        fever.InitializeFeverGuageDecrease();

        VillageManager.Instance.ClosePanelBtn();

        if (!MiddleBossStageManager.Instance.isMiddleBossStage == true && !BossStageManager.Instance.isBossStage == true)
        {
            StageManager.Instance.currentStageLevel++;
        }
        
        StageManager.Instance.StartStage();

        // 과일 초기화 및 새로 생성
        if(matchFruit == null)
        {
            matchFruit = FindObjectOfType<MatchFruit>();
        }

        matchFruit.ResetFruit();
        FruitManager.Instance.ResetAllFruit();
        Tanghulu.Instance.ClearTanghulu();

        // 골드관련 초기화
        GoldManager.Instance.UpdateGoldUI();
        startingGold = GoldManager.Instance.Gold;
        earnedGold = 0;
        expenditureGold = 0;
        additionalDeliveredGold = 0;
        closingGold = 0;

        CustomerManager.Instance.DayCustomerReset();

        TimeManager.Instance.ResetTimer();

        if (!MiddleBossStageManager.Instance.isMiddleBossStage == true && !BossStageManager.Instance.isBossStage == true)
        {
            // 새로운 손님 소환
            StartCoroutine(CustomerManager.Instance.DelaySpawnCustomer(2.5f));
        }

        // 셔터 업
        Shutter.Instance.DelayShutterUp(1f);
        StartCoroutine(InputManager.Instance.LateBlockInputCoroutine(2.5f));
        TimeManager.Instance.DelayStartTimer(2.85f);
    }

    public void InitializeNewGame()
    {
        UpgradeManager.Instance.ResetUpgrades();
        FruitManager.Instance.ResetFruitUpgrades();
        FruitManager.Instance.InitializeFruitData();
        GoldManager.Instance.SetGold(100);
        startingGold = GoldManager.Instance.Gold;

        StartNextRound();
    }

    public void LoadGameData()
    {
        if (string.IsNullOrEmpty(selectedSlot))
        {
            return;
        }

        // 골드와 레벨 관련 데이터 로드
        GoldManager.Instance.SetGold(ES3.Load<int>("Gold", $"{selectedSlot}.es3", defaultValue: 100));
        StageManager.Instance.currentStageLevel = ES3.Load<int>("CurrentStageLevel", $"{selectedSlot}.es3", defaultValue: 1);
        LevelManager.Instance.currentLevel = ES3.Load<int>("CurrentLevel", $"{selectedSlot}.es3", defaultValue: 1);
        LevelManager.Instance.currentExp = ES3.Load<int>("CurrentExp", $"{selectedSlot}.es3", defaultValue: 0);
        LevelManager.Instance.expToNextLevel = ES3.Load<int>("ExpToNextLevel", $"{selectedSlot}.es3", defaultValue: 100);

        // 업그레이드 데이터 로드
        LoadAvailableUpgrades();

        // 과일 데이터 로드
        LoadFruitData();

        // 상점 및 과일 업그레이드 재적용
        UpgradeManager.Instance.RoadSavedUpgradeData();
        FruitManager.Instance.InitializeFruitData();

        // 보스스테이지 클리어 데이터 로드
        LoadBossStageClearData();

        Debug.Log("게임 데이터가 로드되었습니다.");
    }

    public void SaveGameData()
    {
        if (string.IsNullOrEmpty(selectedSlot))
        {
            return;
        }

        // 데이터를 저장
        ES3.Save("Gold", GoldManager.Instance.Gold, $"{selectedSlot}.es3"); // 골드 저장
        ES3.Save("Stage", StageManager.Instance.currentStageLevel, $"{selectedSlot}.es3"); // 스테이지 저장
        ES3.Save("CurrentStageLevel", StageManager.Instance.currentStageLevel, $"{selectedSlot}.es3"); // 현재 스테이지 레벨 저장
        ES3.Save("CurrentLevel", LevelManager.Instance.currentLevel, $"{selectedSlot}.es3"); // 현재 레벨 저장
        ES3.Save("CurrentExp", LevelManager.Instance.currentExp, $"{selectedSlot}.es3"); // 현재 경험치 저장
        ES3.Save("ExpToNextLevel", LevelManager.Instance.expToNextLevel, $"{selectedSlot}.es3"); // 다음 레벨까지 필요한 경험치 저장

        // 업그레이드 데이터 저장
        SaveAvailableUpgrades();
        SaveFruitData();

        Debug.Log("게임 데이터가 저장되었습니다.");
    }

    private void SaveAvailableUpgrades()
    {
        var availableUpgrades = UpgradeManager.Instance.availableUpgrades;

        // UpgradeData의 필드 저장
        for (int i = 0; i < availableUpgrades.Length; i++)
        {
            string upgradeKey = $"UpgradeData_{i}";
            ES3.Save($"{upgradeKey}_Name", availableUpgrades[i].upgradeName, $"{selectedSlot}.es3");
            ES3.Save($"{upgradeKey}_Level", availableUpgrades[i].level, $"{selectedSlot}.es3");
            ES3.Save($"{upgradeKey}_CurrentRate", availableUpgrades[i].currentRate, $"{selectedSlot}.es3");
            ES3.Save($"{upgradeKey}_IncreaseRate", availableUpgrades[i].increaseRate, $"{selectedSlot}.es3");
            ES3.Save($"{upgradeKey}_Gold", availableUpgrades[i].gold, $"{selectedSlot}.es3");
        }
    }

    private void LoadAvailableUpgrades()
    {
        var availableUpgrades = UpgradeManager.Instance.availableUpgrades;

        // UpgradeData의 필드 복원
        for (int i = 0; i < availableUpgrades.Length; i++)
        {
            string upgradeKey = $"UpgradeData_{i}";
            availableUpgrades[i].upgradeName = ES3.Load<string>($"{upgradeKey}_Name", $"{selectedSlot}.es3");
            availableUpgrades[i].level = ES3.Load<int>($"{upgradeKey}_Level", $"{selectedSlot}.es3");
            availableUpgrades[i].currentRate = ES3.Load<int>($"{upgradeKey}_CurrentRate", $"{selectedSlot}.es3");
            availableUpgrades[i].increaseRate = ES3.Load<int>($"{upgradeKey}_IncreaseRate", $"{selectedSlot}.es3");
            availableUpgrades[i].gold = ES3.Load<int>($"{upgradeKey}_Gold", $"{selectedSlot}.es3");
        }
        
        UpgradeManager.Instance.UpdateCurrentShop();
    }

    private void SaveFruitData()
    {
        var currentFruitDataList = FruitManager.Instance.CurrentFruitDataList;

        // FruitData의 필드 저장
        for (int i = 0; i < currentFruitDataList.Count; i++)
        {
            string fruitKey = $"FruitData_{i}";
            ES3.Save($"{fruitKey}_Name", currentFruitDataList[i].fruitName, $"{selectedSlot}.es3");
            ES3.Save($"{fruitKey}_Index", currentFruitDataList[i].fruitIndex, $"{selectedSlot}.es3");
            ES3.Save($"{fruitKey}_Level", currentFruitDataList[i].fruitLevel, $"{selectedSlot}.es3");
            ES3.Save($"{fruitKey}_InitialPrice", currentFruitDataList[i].initialFruitPrice, $"{selectedSlot}.es3");
            ES3.Save($"{fruitKey}_Price", currentFruitDataList[i].fruitPrice, $"{selectedSlot}.es3");
            ES3.Save($"{fruitKey}_IncreaseRate", currentFruitDataList[i].increaseRate, $"{selectedSlot}.es3");
            ES3.Save($"{fruitKey}_UpgradeCost", currentFruitDataList[i].upgradeCost, $"{selectedSlot}.es3");
        }
    }

    private void LoadFruitData()
    {
        // FruitManager의 현재 과일 리스트 초기화
        FruitManager.Instance.CurrentFruitDataList.Clear();

        // 저장된 데이터 로드 및 CurrentFruitDataList에 추가
        for (int i = 0; i < FruitManager.Instance.EntireFruitDataList.Count; i++)
        {
            string fruitKey = $"FruitData_{i}";

            if (ES3.KeyExists($"{fruitKey}_Index", $"{selectedSlot}.es3"))
            {
                int fruitIndex = ES3.Load<int>($"{fruitKey}_Index", $"{selectedSlot}.es3");
                FruitData loadedFruit = FruitManager.Instance.EntireFruitDataList.Find(fruit => fruit.fruitIndex == fruitIndex);

                if (loadedFruit != null)
                {
                    // 저장된 데이터를 현재 과일 리스트에 추가
                    loadedFruit.fruitName = ES3.Load<string>($"{fruitKey}_Name", $"{selectedSlot}.es3");
                    loadedFruit.fruitLevel = ES3.Load<int>($"{fruitKey}_Level", $"{selectedSlot}.es3");
                    loadedFruit.initialFruitPrice = ES3.Load<int>($"{fruitKey}_InitialPrice", $"{selectedSlot}.es3");
                    loadedFruit.fruitPrice = ES3.Load<int>($"{fruitKey}_Price", $"{selectedSlot}.es3");
                    loadedFruit.increaseRate = ES3.Load<int>($"{fruitKey}_IncreaseRate", $"{selectedSlot}.es3");
                    loadedFruit.upgradeCost = ES3.Load<int>($"{fruitKey}_UpgradeCost", $"{selectedSlot}.es3");

                    FruitManager.Instance.CurrentFruitDataList.Add(loadedFruit);
                }
            }
        }

        Debug.Log("Fruit 데이터가 성공적으로 로드되었습니다.");
    }

    private void SaveBossStageClearData()
    {
        for (int i = 0; i < bossStageCleared.Length; i++)
        {
            ES3.Save($"BossStageClear_{i}", bossStageCleared[i], $"{selectedSlot}.es3");
        }
    }

    private void LoadBossStageClearData()
    {
        for (int i = 0; i < bossStageCleared.Length; i++)
        {
            bossStageCleared[i] = ES3.Load($"BossStageClear_{i}", $"{selectedSlot}.es3", defaultValue: false);
        }
    }

    public bool IsBossStageCleared(int index)
    {
        return bossStageCleared[index];
    }

    public void SetBossStageCleared(int index)
    {
        bossStageCleared[index] = true;
        SaveBossStageClearData();
    }
}