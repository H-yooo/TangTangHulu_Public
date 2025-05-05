using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundManager : GenericSingleton<SoundManager>
{
    public AudioSource bgmSource;
    public List<AudioSource> sfxSources;
    public int maxSFXCount = 20; // ���� ��� ������ SFX ��
    public int currentSFXIndex = 0;
    private int currentPlayingSFXCount = 0; // ���� ��� ���� SFX ��
    private float bgmVolume = 0.5f; // BGM ������ ������ ���� (�ʱ� ���� 0.5)
    private float sfxVolume = 0.5f; // SFX ������ ������ ���� (�ʱ� ���� 0.5)

    public List<AudioClip> bgmClips;
    public List<AudioClip> sfxClips;

    private Coroutine currentFadeCoroutine = null; // ���� ����ϰ��� Ư�� �� BGM ��ȯ�� ���������� �Ͼ �� �ڷ�ƾ �ߺ� ȣ�� ����
    private float globalPitch = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        InitializeAudioSource();
    }

    private void InitializeAudioSource()
    {
        //BGM AudioSource �ʱ�ȭ
        GameObject bgmObject = new GameObject("BGMSource");
        bgmObject.transform.parent = transform;
        bgmSource = bgmObject.AddComponent<AudioSource>();
        bgmSource.volume = bgmVolume; // �ʱ� ���� ����
        bgmSource.loop = true;

        // SFX AudioSource �ʱ�ȭ
        sfxSources = new List<AudioSource>();
        for (int i = 0; i < maxSFXCount; i++)
        {
            GameObject sfxObject = new GameObject($"SFXSource_{i}");
            sfxObject.transform.parent = transform;
            AudioSource sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.volume = sfxVolume; // �ʱ� ���� ����
            sfxSource.loop = false;
            sfxSources.Add(sfxSource);
        }
    }
    
    // Option���� ����
    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void PlayBGM(string bgmName, float waitAfterFade = 1.85f)
    {
        AudioClip bgmClip = bgmClips.Find(clip => clip.name == bgmName);
        if (bgmClip == null) return;

        float targetVolume = bgmSource.volume;

        // ���� Fade �ڷ�ƾ�� ���� ���̸� �ߴ�
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }

        if (bgmSource.isPlaying)
        {
            StartCoroutine(FadeOutBGM(bgmSource, 1f, () => { bgmSource.clip = bgmClip; StartCoroutine(FadeInBGM(bgmSource, 1f, targetVolume)); }, waitAfterFade));
        }
        else
        {
            bgmSource.clip = bgmClip;
            StartCoroutine(FadeInBGM(bgmSource, 1f, targetVolume));
        }
    }

    public void PlaySFX(string sfxName, float pitch = -1f) // -1�̸� globalPitch ���
    {
        // ���� ��� ���� SFX�� ���� �ִ�ġ�� �ʰ��ϸ� ���ο� SFX ����� ����
        if (currentPlayingSFXCount >= maxSFXCount) return;

        AudioClip sfxClip = sfxClips.Find(clip => clip.name == sfxName);
        if (sfxClip == null) return;

        AudioSource sfxSource = sfxSources[currentSFXIndex]; // ���� ���� ����
        float originalPitch = sfxSource.pitch; // ���� pitch ���� ����

        // pitch ���� ����
        float finalPitch = (pitch < 0f) ? globalPitch : pitch;
        sfxSource.pitch = finalPitch;

        // �� Clip ������
        sfxSource.clip = sfxClip;
        sfxSource.Play();

        // ��� ���� SFX �� ����
        currentPlayingSFXCount++;
        StartCoroutine(CheckIfPlaying(sfxSource, originalPitch));

        // �ε����� �������� �̵�, �ִ� �ε����� �����ϸ� �ٽ� 0���� �ǵ��ư���.
        currentSFXIndex = (currentSFXIndex + 1) % maxSFXCount;
    }

    // System.Action : �Ű������� ���� ��ȯ���� ���� delegateŸ������, �ڵ��� Ư�� �κп��� Ư�� �۾��� �����ϱ� ���� ���
    // �޼���, �͸� �޼���, ���ٽ� �� � �ڵ� �����̵� ���� �� ������ �Ʒ������� ���̵� �ƿ��� �Ϸ�Ǿ��� �� ����� �۾����� ���ٽ����� ����
    private IEnumerator FadeOutBGM(AudioSource audioSource, float duration, System.Action onComplete, float waitAfterFade = 1.85f)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
        currentFadeCoroutine = null;

        yield return new WaitForSeconds(waitAfterFade);

        onComplete?.Invoke();
    }

    private IEnumerator FadeInBGM(AudioSource audioSource, float duration, float targetVolume)
    {
        audioSource.volume = targetVolume;
        audioSource.Play();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, targetVolume, t / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    // BGM ���� ���� �޼���
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    // SFX ���� ���� �޼���
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume; // ���� ���� ������Ʈ
        foreach (var sfxSource in sfxSources)
        {
            sfxSource.volume = volume;
        }
    }

    private IEnumerator CheckIfPlaying(AudioSource source, float originalPitch)
    {
        while (source.isPlaying)
        {
            yield return null;
        }

        // ����� ���� �� pitch ���� ������� ����
        source.pitch = originalPitch;
        currentPlayingSFXCount--;
    }

    public void SetGlobalPitch(float pitch)
    {
        globalPitch = pitch;

        if (bgmSource != null)
            bgmSource.pitch = pitch;

        foreach (AudioSource sfx in sfxSources)
            sfx.pitch = pitch; // ��� ���� �Ϳ� �ٷ� ����
    }
}