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

        // 슬라이더 값이 바뀌면 볼륨 조정
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            SoundManager.Instance.SetBGMVolume(value);
        });

        sfxSlider.onValueChanged.AddListener((value) =>
        {
            SoundManager.Instance.SetSFXVolume(value);

            // 일정 시간 간격 두고 효과음 재생
            if (Time.time - lastSFXPlayTime > sfxFeedbackCooldown)
            {
                SoundManager.Instance.PlaySFX("Click");
                lastSFXPlayTime = Time.time;
            }
        });
    }
}
