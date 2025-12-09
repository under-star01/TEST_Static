using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static AudioManager Instance { get; private set; }

    [Header("오디오소스")]
    [SerializeField] private AudioSource bgmSource;      // BGM 전용
    [SerializeField] private AudioSource sfxSource;      // 효과음 전용
    [SerializeField] private AudioSource loopSfxSource;  // 루프 효과음 전용

    [Header("BGM")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip SelectBGM;
    [SerializeField] private AudioClip stageBGM;

    [Header("스킬 소리")]
    [SerializeField] private AudioClip TeleportSFX;
    [SerializeField] private AudioClip ScailDownSFX;
    [SerializeField] private AudioClip ScailReturnSFX;
    [SerializeField] private AudioClip IstriggerSFX;
    [SerializeField] private AudioClip AddForceSFX;
    [SerializeField] private AudioClip IstriggerEndSFX;
    [SerializeField] private AudioClip CloneSFX;
    [SerializeField] private AudioClip YieldReturnSFX;
    [SerializeField] private AudioClip SlowSFX;
    [SerializeField] private AudioClip HitSFX;
    [SerializeField] private AudioClip GetItemSFX;





    [Header("UI 소리")]
    [SerializeField] private AudioClip menuSelectSFX;
    [SerializeField] private AudioClip ButtonSFX;

    [Header("볼륨")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 초기화
        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
        }

        if (loopSfxSource != null)
        {
            loopSfxSource.loop = true;
            loopSfxSource.volume = sfxVolume;
        }
    }

    private void Update()
    {
        // 볼륨 실시간 적용
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }


    // BGM 재생
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // BGM 정지
    public void StopBGM()
    {
        if (bgmSource == null) return;

        bgmSource.Stop();
    }

    // BGM 일시정지
    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    // BGM 재개
    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    // 효과음

    //효과음 재생 (기본)
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // 효과음 재생 (볼륨 조절)
    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // 효과음 

    public void PlayTeleportSFX() => PlaySFX(TeleportSFX);
    public void PlayScailDownSFX() => PlaySFX(ScailDownSFX);
    public void PlayScailReturnSFX() => PlaySFX(ScailReturnSFX);
    public void PlayIstriggerSFX() => PlaySFX(IstriggerSFX);
    public void PlayIstriggerEndSFX() => PlaySFX(IstriggerEndSFX);
    public void PlayAddForceSFX() => PlaySFX(AddForceSFX);
    public void PlayCloneSFX() => PlaySFX(CloneSFX);
    public void PlayYieldReturnSFX() => PlaySFX(YieldReturnSFX);
    public void PlaySlowSFX() => PlaySFX(SlowSFX);
    public void PlayHitSFX() => PlaySFX(HitSFX);
    public void PlayGetItemSFX() => PlaySFX(GetItemSFX);




    //BGM
    public void PlayTitleBGM() => PlayBGM(titleBGM);
    public void PlaySelectBGM() => PlayBGM(SelectBGM);
    public void PlayStageBGM() => PlayBGM(stageBGM);






    //UI
    public void PlayMenuSelectSFX() => PlaySFX(menuSelectSFX);
    public void PlayButtonSFX() => PlaySFX(ButtonSFX);

    


    // 유틸리티

    // 모든 소리 정지
    public void StopAll()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
    }

    //볼륨 설정
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    // 루프 효과음

    // 루프 효과음 재생 시작
    public void PlayLoopSFX(AudioClip clip)
    {
        if (loopSfxSource == null || clip == null) return;

        loopSfxSource.clip = clip;
        loopSfxSource.Play();
    }

    // 루프 효과음 정지
    public void StopLoopSFX()
    {
        if (loopSfxSource == null) return;
        loopSfxSource.Stop();
    }
}
