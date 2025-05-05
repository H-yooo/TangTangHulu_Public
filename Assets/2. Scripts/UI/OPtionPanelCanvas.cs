using UnityEngine;
using UnityEngine.UI;

public class OPtionPanelCanvas : GenericSingleton<OPtionPanelCanvas>
{
    [SerializeField] private Fever fever;

    [SerializeField] private GameObject OptionPanel;
    [SerializeField] private Button LobbyOpenPanelBtn;
    [SerializeField] private Button GameOpenPanelBtn;
    [SerializeField] private Button ClosePanelBtn;

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private float sfxFeedbackCooldown = 0.5f;
    private float lastSFXPlayTime = -1f;

    private void Start()
    {
        LobbyOpenPanelBtn.onClick.AddListener(OpenOptionPanel);
        GameOpenPanelBtn.onClick.AddListener(OpenOptionPanel);
        ClosePanelBtn.onClick.AddListener(CloseOptionPanel);

        bgmSlider.value = SoundManager.Instance.GetBGMVolume();
        sfxSlider.value = SoundManager.Instance.GetSFXVolume();

        // �����̴� ���� �ٲ�� ���� ����
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            SoundManager.Instance.SetBGMVolume(value);
        });

        sfxSlider.onValueChanged.AddListener((value) =>
        {
            SoundManager.Instance.SetSFXVolume(value);

            // ���� �ð� ���� �ΰ� ȿ���� ���
            if (Time.time - lastSFXPlayTime > sfxFeedbackCooldown)
            {
                SoundManager.Instance.PlaySFX("Click");
                lastSFXPlayTime = Time.time;
            }
        });
    }

    public void OpenOptionPanel()
    {
        SoundManager.Instance.PlaySFX("Click");
        OptionPanel.SetActive(true);

        // Ÿ�̸ӿ� �Է��� �����
        TimeManager.Instance.IsTimerRunning = false;
        InputManager.Instance.BlockInput(true);
        fever.IsPaused = true;
    }

    public void CloseOptionPanel()
    {
        SoundManager.Instance.PlaySFX("Click");
        OptionPanel.SetActive(false);

        // Ÿ�̸ӿ� �Է��� �簳�Ѵ�
        if (TimeManager.Instance.GetStageTime() > 0f)
        {
            TimeManager.Instance.IsTimerRunning = true;
        }
        
        InputManager.Instance.BlockInput(false);
        fever.IsPaused = false;
    }
}