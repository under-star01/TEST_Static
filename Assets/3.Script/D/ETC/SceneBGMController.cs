using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBGMController : MonoBehaviour
{
    [Header("씬 BGM 설정")]
    [SerializeField] private BGMType bgmType = BGMType.Stage;

    public enum BGMType
    {
        Title,      // 타이틀 화면
        Select,     // 선택 화면
        Stage       // 스테이지
    }

    private void Start()
    {
        PlayBGMForCurrentScene();
    }

    private void PlayBGMForCurrentScene()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager가 없습니다!");
            return;
        }

        switch (bgmType)
        {
            case BGMType.Title:
                AudioManager.Instance.PlayTitleBGM();
                break;

            case BGMType.Select:
                AudioManager.Instance.PlaySelectBGM();
                break;

            case BGMType.Stage:
                AudioManager.Instance.PlayStageBGM();
                break;
        }

        Debug.Log($"{bgmType} BGM 재생!");
    }
}
