using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyCanvas : MonoBehaviour
{
    public GameObject title;
    [SerializeField] private Button loadGameBtn;
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button optionBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private GameObject optionPanel;

    [Header("Panels")]
    [SerializeField] private GameObject saveSlotSelectionPanel;
    [SerializeField] private GameObject marketNamePanel;

    [Header("Slot Selection")]
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private Button closeSlotPanelBtn;
    [SerializeField] private TMP_Text[] slotTexts;

    [Header("Market Name")]
    [SerializeField] private TMP_InputField marketNameInput;
    [SerializeField] private Button openMarketBtn;

    private string selectedSlot;
    private bool isNewGame;

    private void Start()
    {
        loadGameBtn.onClick.AddListener(OnLoadGameBtnClicked);
        newGameBtn.onClick.AddListener(OnNewGameBtnClicked);
        optionBtn.onClick.AddListener(OnOptionBtnClicked);
        quitBtn.onClick.AddListener(OnQuitBtnClicked);

        closeSlotPanelBtn.onClick.AddListener(CloseSlotSelectionPanel);

        UpdateSlotTexts(); // 저장된 데이터를 확인하여 텍스트 업데이트
    }

    private void OnLoadGameBtnClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        isNewGame = false;
        saveSlotSelectionPanel.SetActive(true);
        DisableButtons();
    }

    private void OnNewGameBtnClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        isNewGame = true;
        saveSlotSelectionPanel.SetActive(true);
        DisableButtons();
    }
  

    public void OnOptionBtnClicked()
    {
        OPtionPanelCanvas.Instance.OpenOptionPanel();
    }

    private void OnQuitBtnClicked()
    {
        SoundManager.Instance.PlaySFX("Click");
        Application.Quit();
    }

    public void OnSlotSelected(string slotName)
    {
        SoundManager.Instance.PlaySFX("Click");
        selectedSlot = slotName;
        GameManager.Instance.SetSelectedSlot(slotName); // GameManager에 슬롯 이름 전달

        if (isNewGame) // 새 게임 모드
        {
            if (ES3.FileExists($"{slotName}.es3"))
            {
                Debug.Log($"{slotName}.es3 파일이 존재합니다."); // 존재 여부 확인
                NotificationCanvas.Instance.ShowNotificationPanel2("선택한 슬롯에는 이미 저장된 데이터가 있습니다. 삭제하시겠습니까?",ConfirmSlotOverride);
            }
            else
            {
                // 데이터가 없으면 바로 가게 이름 입력 창 표시
                ShowMarketNamePanel();
            }
        }
        else // 이어하기 모드
        {
            if (ES3.FileExists($"{slotName}.es3"))
            {
                string marketName = ES3.Load<string>("MarketName", $"{slotName}.es3", defaultValue: "Unnamed Slot");
                NotificationCanvas.Instance.ShowNotificationPanel2(
                    $"해당 슬롯의 '{marketName}' 데이터를 로드하시겠습니까?",
                    () =>
                    {
                        GameManager.Instance.LoadGameData();
                        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");

                        if (StageManager.Instance.currentStageLevel == 0)
                        {
                            saveSlotSelectionPanel.SetActive(false);
                            GameManager.Instance.InitializeNewGame();
                            title.SetActive(false);
                            InputManager.Instance.BlockInput(false);
                        }
                        else
                        {
                            saveSlotSelectionPanel.SetActive(false);
                            title.SetActive(false);
                            InputManager.Instance.BlockInput(false);
                            EnableButtons();
                            VillageManager.Instance.OpenVillage();
                        }
                    }
                );
            }
            else
            {
                NotificationCanvas.Instance.ShowNotificationPanel1("해당 슬롯에는 저장 데이터가 없습니다.");
            }
        }
    }

    private void ConfirmSlotOverride()
    {
        // 슬롯 덮어쓰기를 확인한 경우 가게 이름 입력 창 표시
        ShowMarketNamePanel();
    }

    private void ShowMarketNamePanel()
    {
        saveSlotSelectionPanel.SetActive(false);
        marketNamePanel.SetActive(true);
    }

    public void OpenMarket()
    {
        // 입력된 가게 이름 저장, 기본 이름은 탕탕후루
        string marketName = string.IsNullOrEmpty(marketNameInput.text) ? "탕탕후루" : marketNameInput.text;
        ES3.Save("MarketName", marketName, $"{selectedSlot}.es3");
        GameManager.Instance.SaveGameData();
        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");

        marketNamePanel.SetActive(false);
        EnableButtons();

        // 게임 시작
        GameManager.Instance.InitializeNewGame();
        title.SetActive(false);
        InputManager.Instance.BlockInput(false);
    }

    private void CloseSlotSelectionPanel()
    {
        // 슬롯 선택 창 닫기
        SoundManager.Instance.PlaySFX("Click");
        saveSlotSelectionPanel.SetActive(false);
        EnableButtons();
    }

    private void DisableButtons()
    {
        loadGameBtn.interactable = false;
        newGameBtn.interactable = false;
        optionBtn.interactable = false;
        quitBtn.interactable = false;
    }

    private void EnableButtons()
    {
        loadGameBtn.interactable = true;
        newGameBtn.interactable = true;
        optionBtn.interactable = true;
        quitBtn.interactable = true;
    }

    private void UpdateSlotTexts()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            string slotName = $"Slot{i + 1}.es3";

            if (ES3.FileExists(slotName)) // 해당 슬롯에 데이터가 있는지 확인
            {
                string marketName = ES3.Load<string>("MarketName", slotName, defaultValue: "Empty Slot");
                slotTexts[i].text = marketName;
                slotTexts[i].color = new Color(slotTexts[i].color.r, slotTexts[i].color.g, slotTexts[i].color.b, 1f);
            }
            else
            {
                slotTexts[i].text = "빈 슬롯";
                slotTexts[i].color = new Color(slotTexts[i].color.r, slotTexts[i].color.g, slotTexts[i].color.b, 0.5f);
            }
        }
    }
}