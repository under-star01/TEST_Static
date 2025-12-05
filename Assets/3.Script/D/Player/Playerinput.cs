using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Playerinput : MonoBehaviour
{
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // 우클릭 입력을 감지하는 콜백 등록
        inputActions.Player.RightClick.performed += OnRightClick;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.RightClick.performed -= OnRightClick;
        inputActions.Disable();
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        Debug.Log("입력됨.");
    }
}
