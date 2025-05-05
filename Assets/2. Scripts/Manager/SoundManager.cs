using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundManager : GenericSingleton<SoundManager>
{
    public AudioSource bgmSource;
    public List<AudioSource> sfxSources;
    public int maxSFXCount = 20; // 동시 재생 가능한 SFX 수
    public int currentSFXIndex = 0;
    private int currentPlayingSFXCount = 0; // 현재 재생 중인 SFX 수
    private float bgmVolume = 0.5f; // BGM 볼륨을 저장할 변수 (초기 볼륨 0.5)
    private float sfxVolume = 0.5f; // SFX 볼륨을 저장할 변수 (초기 볼륨 0.5)

    public List<AudioClip> bgmClips;
    public List<AudioClip> sfxClips;

    private Coroutine currentFadeCoroutine = null; // 빠른 모바일게임 특성 상 BGM 전환이 연속적으로 일어날 때 코루틴 중복 호출 방지
    private float globalPitch = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        InitializeAudioSource();
    }

    private void InitializeAudioSource()
    {
        //BGM AudioSource 초기화
        GameObject bgmObject = new GameObject("BGMSource");
        bgmObject.transform.parent = transform;
        bgmSource = bgmObject.AddComponent<AudioSource>();
        bgmSource.volume = bgmVolume; // 초기 볼륨 설정
        bgmSource.loop = true;

        // SFX AudioSource 초기화
        sfxSources = new List<AudioSource>();
        for (int i = 0; i < maxSFXCount; i++)
        {
            GameObject sfxObject = new GameObject($"SFXSource_{i}");
            sfxObject.transform.parent = transform;
            AudioSource sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.volume = sfxVolume; // 초기 볼륨 설정
            sfxSource.loop = false;
            sfxSources.Add(sfxSource);
        }
    }
    
    // Option으로 전달
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

        // 기존 Fade 코루틴이 실행 중이면 중단
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

    public void PlaySFX(string sfxName, float pitch = -1f) // -1이면 globalPitch 사용
    {
        // 현재 재생 중인 SFX의 수가 최대치를 초과하면 새로운 SFX 재생을 무시
        if (currentPlayingSFXCount >= maxSFXCount) return;

        AudioClip sfxClip = sfxClips.Find(clip => clip.name == sfxName);
        if (sfxClip == null) return;

        AudioSource sfxSource = sfxSources[currentSFXIndex]; // 현재 볼륨 설정
        float originalPitch = sfxSource.pitch; // 원래 pitch 값을 저장

        // pitch 값을 변경
        float finalPitch = (pitch < 0f) ? globalPitch : pitch;
        sfxSource.pitch = finalPitch;

        // 새 Clip 끼워줌
        sfxSource.clip = sfxClip;
        sfxSource.Play();

        // 재생 중인 SFX 수 증가
        currentPlayingSFXCount++;
        StartCoroutine(CheckIfPlaying(sfxSource, originalPitch));

        // 인덱스를 다음으로 이동, 최대 인덱스에 도달하면 다시 0으로 되돌아간다.
        currentSFXIndex = (currentSFXIndex + 1) % maxSFXCount;
    }

    // System.Action : 매개변수가 없고 반환값도 없는 delegate타입으로, 코드의 특정 부분에서 특정 작업을 수행하기 위해 사용
    // 메서드, 익명 메서드, 람다식 등 어떤 코드 조각이든 받을 수 있으며 아래에서는 페이드 아웃이 완료되었을 때 실행될 작업으로 람다식으로 전달
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

    // BGM 볼륨 조절 메서드
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    // SFX 볼륨 조절 메서드
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume; // 볼륨 변수 업데이트
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

        // 재생이 끝난 후 pitch 값을 원래대로 복원
        source.pitch = originalPitch;
        currentPlayingSFXCount--;
    }

    public void SetGlobalPitch(float pitch)
    {
        globalPitch = pitch;

        if (bgmSource != null)
            bgmSource.pitch = pitch;

        foreach (AudioSource sfx in sfxSources)
            sfx.pitch = pitch; // 재생 중인 것에 바로 적용
    }
}