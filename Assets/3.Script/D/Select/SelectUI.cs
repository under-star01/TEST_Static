using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectUI : MonoBehaviour
{
    public CharacterSelectCamera selector;
    public Button leftButton;
    public Button rightButton;

    void Start()
    {
        leftButton.onClick.AddListener(() => selector.SelectLeft());
        rightButton.onClick.AddListener(() => selector.SelectRight());
    }
}
