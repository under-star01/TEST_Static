using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput_D : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask detectLayer;

    private PlayerInput_DD inputActions;
    private PlayerMove_D playerMove;

    // 두 가지 스킬 컨트롤러 (하나만 사용)
    private TransformSkill_D transformSkill;
    private CoroutineSkill coroutineSkill;

    private void Awake()
    {
        // 컴포넌트 연결
        TryGetComponent(out playerMove);
        TryGetComponent(out transformSkill);
        TryGetComponent(out coroutineSkill);

        // InputAction 생성
        inputActions = new PlayerInput_DD();

        // 카메라 연결
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 어떤 스킬 컨트롤러를 사용하는지 확인
        if (transformSkill != null)
        {
            Debug.Log("TransformSkill_D 감지 - 텔레포트/작아지기 캐릭터");
        }
        if (coroutineSkill != null)
        {
            Debug.Log("CoroutineSkill 감지 - 시간 정지 캐릭터");
        }
    }

    private void OnEnable()
    {
        // 이동 및 회전 InputAction 연결 및 활성화
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;

        // 스킬 InputAction 연결
        inputActions.Player.Skill1.started += OnSkill1Started;      // 좌클릭 시작
        inputActions.Player.Skill2.performed += OnSkill2;           // Shift

        inputActions.Enable();
    }

    private void OnDisable()
    {
        // 이동 및 회전 InputAction 해제 및 비활성화
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Look.performed -= OnLook;

        // 스킬 InputAction 해제
        inputActions.Player.Skill1.started -= OnSkill1Started;
        inputActions.Player.Skill2.performed -= OnSkill2;

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

    // 스킬1 (캐릭터에 따라 다름)
    private void OnSkill1Started(InputAction.CallbackContext context)
    {
        // TransformSkill_D가 있으면 텔레포트
        if (transformSkill != null)
        {
            transformSkill.OnTeleportStart();
        }
        // CoroutineSkill이 있으면 시간 정지
        else if (coroutineSkill != null)
        {
            coroutineSkill.OnTimeStopSkill();
        }
    }

    // 스킬2 (캐릭터에 따라 다름)
    private void OnSkill2(InputAction.CallbackContext context)
    {
        // TransformSkill_D가 있으면 작아지기
        if (transformSkill != null)
        {
            transformSkill.OnShrinkSkill();
        }
        // CoroutineSkill이 있으면 스킬2
        else if (coroutineSkill != null)
        {
            coroutineSkill.OnSkill2();
        }
    }
}