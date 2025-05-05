using UnityEngine;
using UnityEngine.UI;

public class OptionPanel : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private float sfxFeedbackCooldown = 0.5f;
    private float lastSFXPlayTime = -1f;

    private void Start()
    {
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
}
