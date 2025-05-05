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

        UpdateSlotTexts(); // ����� �����͸� Ȯ���Ͽ� �ؽ�Ʈ ������Ʈ
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
        GameManager.Instance.SetSelectedSlot(slotName); // GameManager�� ���� �̸� ����

        if (isNewGame) // �� ���� ���
        {
            if (ES3.FileExists($"{slotName}.es3"))
            {
                Debug.Log($"{slotName}.es3 ������ �����մϴ�."); // ���� ���� Ȯ��
                NotificationCanvas.Instance.ShowNotificationPanel2("������ ���Կ��� �̹� ����� �����Ͱ� �ֽ��ϴ�. �����Ͻðڽ��ϱ�?",ConfirmSlotOverride);
            }
            else
            {
                // �����Ͱ� ������ �ٷ� ���� �̸� �Է� â ǥ��
                ShowMarketNamePanel();
            }
        }
        else // �̾��ϱ� ���
        {
            if (ES3.FileExists($"{slotName}.es3"))
            {
                string marketName = ES3.Load<string>("MarketName", $"{slotName}.es3", defaultValue: "Unnamed Slot");
                NotificationCanvas.Instance.ShowNotificationPanel2(
                    $"�ش� ������ '{marketName}' �����͸� �ε��Ͻðڽ��ϱ�?",
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
                NotificationCanvas.Instance.ShowNotificationPanel1("�ش� ���Կ��� ���� �����Ͱ� �����ϴ�.");
            }
        }
    }

    private void ConfirmSlotOverride()
    {
        // ���� ����⸦ Ȯ���� ��� ���� �̸� �Է� â ǥ��
        ShowMarketNamePanel();
    }

    private void ShowMarketNamePanel()
    {
        saveSlotSelectionPanel.SetActive(false);
        marketNamePanel.SetActive(true);
    }

    public void OpenMarket()
    {
        // �Էµ� ���� �̸� ����, �⺻ �̸��� �����ķ�
        string marketName = string.IsNullOrEmpty(marketNameInput.text) ? "�����ķ�" : marketNameInput.text;
        ES3.Save("MarketName", marketName, $"{selectedSlot}.es3");
        GameManager.Instance.SaveGameData();
        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");

        marketNamePanel.SetActive(false);
        EnableButtons();

        // ���� ����
        GameManager.Instance.InitializeNewGame();
        title.SetActive(false);
        InputManager.Instance.BlockInput(false);
    }

    private void CloseSlotSelectionPanel()
    {
        // ���� ���� â �ݱ�
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

            if (ES3.FileExists(slotName)) // �ش� ���Կ� �����Ͱ� �ִ��� Ȯ��
            {
                string marketName = ES3.Load<string>("MarketName", slotName, defaultValue: "Empty Slot");
                slotTexts[i].text = marketName;
                slotTexts[i].color = new Color(slotTexts[i].color.r, slotTexts[i].color.g, slotTexts[i].color.b, 1f);
            }
            else
            {
                slotTexts[i].text = "�� ����";
                slotTexts[i].color = new Color(slotTexts[i].color.r, slotTexts[i].color.g, slotTexts[i].color.b, 0.5f);
            }
        }
    }
}