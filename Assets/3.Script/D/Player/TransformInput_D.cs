using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransformInput_D : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask detectLayer;

    private TransformInput inputActions;
    private PlayerMove_D playerMove;
    private TransformSkill_D skillController;

    private void Awake()
    {
        // 컴포넌트 연결
        TryGetComponent(out playerMove);
        TryGetComponent(out skillController);

        // InputAction 생성
        inputActions = new TransformInput();

        // 카메라 연결
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        // 이동 및 회전 InputAction 연결 및 활성화
        inputActions.Transform.Move.performed += OnMove;
        inputActions.Transform.Move.canceled += OnMove;
        inputActions.Transform.Look.performed += OnLook;

        // 스킬 InputAction 연결
        inputActions.Transform.Skill1.started += OnSkill1Started;      // 좌클릭 시작
        inputActions.Transform.Skill2.performed += OnSkill2;           // Shift

        inputActions.Enable();
    }

    private void OnDisable()
    {
        // 이동 및 회전 InputAction 해제 및 비활성화
        inputActions.Transform.Move.performed -= OnMove;
        inputActions.Transform.Move.canceled -= OnMove;
        inputActions.Transform.Look.performed -= OnLook;

        // 스킬 InputAction 해제
        inputActions.Transform.Skill1.started -= OnSkill1Started;
        inputActions.Transform.Skill2.performed -= OnSkill2;

        inputActions.Disable();
    }

    // WASD 방향키 이동 메소드
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 raw = context.ReadValue<Vector2>();
        float dead = 0.1f;
        float dirX = 0f;
        float dirY = 0f;

        // 1, 0, -1로 input 정리
        if (raw.x > dead) dirX = 1f;
        if (raw.x < -dead) dirX = -1f;
        if (raw.y > dead) dirY = 1f;
        if (raw.y < -dead) dirY = -1f;

        Vector2 move = new Vector2(dirX, dirY);
        playerMove.SetMoveInput(move);
    }

    // 화면 회전 메소드
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 look = context.ReadValue<Vector2>();
        playerMove.SetLookInput(look);
    }

    // 스킬1 
    private void OnSkill1Started(InputAction.CallbackContext context)
    {
        if (skillController != null)
        {
            skillController.OnTeleportStart();
        }
    }

    // 스킬2 (Shift - 작아지기)
    private void OnSkill2(InputAction.CallbackContext context)
    {
        if (skillController != null)
        {
            skillController.OnShrinkSkill();
        }
    }
}