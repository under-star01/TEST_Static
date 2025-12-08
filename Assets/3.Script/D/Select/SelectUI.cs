using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SelectUI : MonoBehaviour
{
    [Header("셀렉터")]
    public CharacterSelectCamera selector;

    [Header("화살표버튼")]
    public Button leftButton;
    public Button rightButton;

    [Header("캐릭정보")]
    public TextMeshProUGUI characterNameText; // 이름
    public TextMeshProUGUI characterDescriptionText; //설명
    public Image characterIconImage; // 아이콘

    [Header("선택버튼")]
    public Button selectButton;

    public void Start()
    {
        leftButton.onClick.AddListener(() => selector.SelectLeft());
        rightButton.onClick.AddListener(() => selector.SelectRight());
        selectButton.onClick.AddListener(OnSelectCharacter);

        // 캐릭터 변경 이벤트 구독
        selector.OnCharacterChanged += UpdateCharacterInfo;

        // 시작 시 현재 캐릭터 정보 표시
        UpdateCharacterInfo(selector.CurrentCharacter);
    }

    public void OnDestroy()
    {
        // 이벤트 구독 해제
        if (selector != null)
            selector.OnCharacterChanged -= UpdateCharacterInfo;
    }

    // 캐릭터 정보 UI 업데이트
    public void UpdateCharacterInfo(CharacterData character)
    {
        if (character == null) return;

        if (characterNameText != null)
            characterNameText.text = character.characterName;

        if (characterDescriptionText != null)
            characterDescriptionText.text = character.characterDescription;

        if (characterIconImage != null && character.characterIcon != null)
        {
            characterIconImage.sprite = character.characterIcon;
            characterIconImage.enabled = true;
        }
    }


    public void OnSelectCharacter()
    {
        CharacterData selectedCharacter = selector.CurrentCharacter;
        int selectedIndex = selector.CurrentIndex;

        Debug.Log($"선택된 캐릭터: {selectedCharacter.characterName} (인덱스: {selectedIndex})");

        // 여기서 선택된 캐릭터 정보를 저장하거나 다음 씬으로 전달
        PlayerPrefs.SetInt("SelectedCharacterIndex", selectedIndex);
        PlayerPrefs.Save();

        // 게임 시작
        SceneManager.LoadScene("A");
    }
}