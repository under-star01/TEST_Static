using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput_A : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private LayerMask detectLayer;

    private PlayerInputActions inputActions;
    private PlayerMove_A playerMove;
    private PlayerSkill_RigidBody playerSkill;

    private void Awake()
    {
        // 컴포넌트 연결
        TryGetComponent(out playerMove);
        TryGetComponent(out playerSkill);

        // InputAction 생성
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // InputAction 연결 및 활성화
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Skill_Shift.performed += OnShiftSkill;
        inputActions.Player.Skill_Space.performed += OnSpaceSkill;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        // InputAction 해제 및 비활성화
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Skill_Shift.performed -= OnShiftSkill;
        inputActions.Player.Skill_Space.performed -= OnSpaceSkill;
        inputActions.Disable();
    }

    // WASD 방향키 이동 메소드
    private void OnMove(InputAction.CallbackContext context)
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
    private void OnLook(InputAction.CallbackContext context)
    {
        Vector2 look = context.ReadValue<Vector2>();
        playerMove.SetLookInput(look);
    }

    // Shift 스킬 사용 메소드
    private void OnShiftSkill(InputAction.CallbackContext context)
    {
        playerSkill.UseSkill_Shift();
    }

    // Space 스킬 사용 메소드
    private void OnSpaceSkill(InputAction.CallbackContext context)
    {
        playerSkill.UseSkill_Space();
    }
}