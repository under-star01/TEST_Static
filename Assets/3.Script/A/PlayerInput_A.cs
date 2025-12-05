using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput_A : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask detectLayer;
    
    private PlayerInputActions inputActions;
    private PlayerMove_A playerMove;

    private void Awake()
    {
        // 컴포넌트 연결
        TryGetComponent(out playerMove);

        // InputAction 생성
        inputActions = new PlayerInputActions();

        // 카메라 연결
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        // InputAction 연결 및 활성화
        inputActions.Player.RightClick.performed += OnRightClick;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        // InputAction 해제 및 비활성화
        inputActions.Player.RightClick.performed -= OnRightClick;
        inputActions.Disable();
    }

    // 마우스 클릭 입력 메소드
    private void OnRightClick(InputAction.CallbackContext context)
    {
        // 클릭 위치로 이동 함수 실행 
        Vector3 screenPos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, detectLayer))
        {
            Vector3 hitPoint = hit.point;

            hitPoint.y = transform.position.y; // y값은 유지
            playerMove.SetDestination(hit.point);
        }
    }


}