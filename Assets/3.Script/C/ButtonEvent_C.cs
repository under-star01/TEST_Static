using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonEvent : MonoBehaviour
{
    public GameObject toggleUI;
    public void SceneLoader(string sceneame)//다른 씬으로
    {
        PlayerPrefs.SetInt("Scoer",0);
        SceneManager.LoadScene(sceneame);
    }
    public void GameQuit() //게임종료
    {
        Application.Quit();
    }
    public void ToggleUI()//UI 패널 활성화/비활성화
    {
        if(toggleUI != null)
        {
            toggleUI.SetActive(!toggleUI.activeSelf);
        }
        else
        {
            Debug.LogError("ToggleUI가 할당되지 않았습니다.");
        }
    }
}
