using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove_A : MonoBehaviour
{
    [Header("맵 범위")]
    [SerializeField] private Collider mapCollider; // 맵 오브젝트 콜라이더

    [Header("화면 회전 관련 설정")]
    [SerializeField] private Transform cameraRoot;   // 카메라 부모 (pitch 회전용)
    [SerializeField] public float lookSensitivity = 150f;
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch = 80f;

    [Header("이동 관련 설정")]
    [SerializeField] public float moveSpeed = 5f; // 이동 속도

    [Header("상태 관련 설정")]
    [SerializeField] public bool isMoveLocked; // 이동 제한 여부

    private Rigidbody rb;
    public Animator animator;
    private Vector3 targetPos;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float minX, minZ, maxX,maxZ; // 맵 크기 변수
   
    // 회전 누적값
    private float yaw;   // y축 회전 (좌우)
    private float pitch; // x축 회전 (위/아래)

    private void Awake()
    {
        // 컴포넌트 연결
        TryGetComponent(out rb);
        animator = GetComponentInChildren<Animator>();

        // 카메라 위아래 기준 오브젝트 설정
        if (cameraRoot == null)
        {
            Debug.LogWarning("cameraRoot가 설정되지 않았습니다.");
        }

        // 맵 범위 적용
        if (mapCollider != null)
        {
            Bounds mapBound = mapCollider.bounds;
            
            minX = mapBound.min.x;
            maxX = mapBound.max.x;
            minZ = mapBound.min.z;
            maxZ = mapBound.max.z;
        }
        else
        {
            Debug.LogWarning("맵 범위 연결이 안됐어용");
        }

        //선택한 캐릭터 불러오기
        PlayerPrefs.GetInt("SelectedCharacterIndex");
    }

    private void Start()
    {
        // 마우스 고정 + 숨김
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 시작 회전값으로 초기화
        yaw = transform.eulerAngles.y;
        if (cameraRoot != null)
            pitch = cameraRoot.localEulerAngles.x;
    }

    private void FixedUpdate()
    {
        // moveLock일 경우 이동 제한
        if (isMoveLocked)
        {
            ClampPositionInsideMap();
            return;
        }
        
        // 이동 적용
        HandleMove();

        // 이동 시, 맵 범위로 제한 적용
        ClampPositionInsideMap();
    }

    private void Update()
    {
        if (isMoveLocked) return;

        // 속도에 따른 애니메이션 적용
        float speed = rb.linearVelocity.magnitude;
        
        animator.SetFloat("Speed", speed);
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);

        // 화면 회전 적용
        HandleLook();
    }

    public void SetMoveInput(Vector2 move)
    {
        moveInput = move;
    }

    public void SetLookInput(Vector2 look)
    {
        lookInput = look;
    }

    private void HandleMove()
    {
        // 현재 바라보는 방향 기준으로 앞/뒤, 좌/우 벡터
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;

        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = rb.linearVelocity.y; // 중력 유지

        rb.linearVelocity = velocity;
    }

    // 화면 회전 메소드
    private void HandleLook()
    {
        if (lookInput == Vector2.zero)
            return;

        // 마우스 델타 기반 회전 누적
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY; // 위로 올릴 때 pitch 감소

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 좌우 회전은 플레이어 전체
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // 상하 회전은 카메라 루트만
        if (cameraRoot != null)
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        
        // 다시 Vector.zero로 초기화
        lookInput = Vector2.zero;
    }

    // 맵 크기로 이동 제한 메소드
    private void ClampPositionInsideMap()
    {
        if (mapCollider == null) return;

        Vector3 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        rb.position = pos;
    }

    // 넉백 실행 메소드
    public void ApplyKnockBack(Vector3 dir, float power, float duration)
    {
        dir.y = 0f;
        rb.linearVelocity = dir.normalized * power;

        // 피격 애니메이션 트리거
        animator.SetTrigger("Damaged");
    }

    // 넉백 종료시 속도 초기화 메소드
    private void HandleKnockbackEnd()
    {
        rb.linearVelocity = Vector3.zero;
    }
}
