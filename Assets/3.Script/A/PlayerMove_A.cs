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
    [SerializeField] public bool isDamaged = false; // 피격 상태 여부
    private Coroutine damageRoutine; // 피격 코루틴 저장

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
        mapCollider = GameObject.FindGameObjectWithTag("Floor").GetComponent<Collider>();

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
        if (isMoveLocked || isDamaged)
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
        if (isMoveLocked || isDamaged) return;

        // 속도에 따른 애니메이션 적용
        float speed = rb.linearVelocity.magnitude;
        
        animator.SetFloat("Speed", speed);
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);

        // 화면 회전 적용
        HandleLook();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 피격 상태일 경우 무시
        if (isDamaged) return;

        if (collision.gameObject.CompareTag("Error"))
        {
            // 피격 데미지 적용
            GameManager.Instance.TakeDamage(2);

            AudioManager.Instance.PlayHitSFX();
            // 반대방향으로, 5f만큼 0.4초 동안 넉백!
            Vector3 dir = transform.position - collision.transform.position;
            dir.y = 0f;

            // 1초 스턴
            ApplyKnockBack(dir.normalized, 5f, 0.4f);
            ActiveDamagedState(1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 피격 상태일 경우 무시
        if (isDamaged) return;

        if (other.gameObject.CompareTag("Warning"))
        {
            // 피격 데미지 적용
            GameManager.Instance.TakeDamage(1);

            AudioManager.Instance.PlayHitSFX();
            // 반대방향으로, 5f만큼 0.4초 동안 넉백!
            Vector3 dir = transform.position - other.transform.position;
            dir.y = 0f;

            // 0.5초 스턴
            ApplyKnockBack(dir.normalized, 5f, 0.4f);
            ActiveDamagedState(0.5f);
        }
        // Caching 아이템 사용
        else if (other.gameObject.CompareTag("Caching"))
        {
            Debug.Log("[아이템] : Caching 사용!");
            AudioManager.Instance.PlayGetItemSFX();
            GameManager.Instance.UseItem_Caching();
            other.gameObject.SetActive(false);
        }
        // URP 아이템 사용
        else if (other.gameObject.CompareTag("URP"))
        {
            Debug.Log("[아이템] : URP 사용!");
            AudioManager.Instance.PlayGetItemSFX();
            GameManager.Instance.UseItem_URP();
            other.gameObject.SetActive(false);
        }
        // GC 아이템 사용
        else if (other.gameObject.CompareTag("GC"))
        {
            Debug.Log("[아이템] : GC 사용!");
            AudioManager.Instance.PlayGetItemSFX();
            GameManager.Instance.UseItem_GC();
            other.gameObject.SetActive(false);
        }
    }

    // 피격 상태 활성화 메소드
    private void ActiveDamagedState(float duration)
    {
        if(damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
        }

        damageRoutine = StartCoroutine(ActiveDamagedState_co(duration));
    }

    // 피격 상태 활성화 코루틴
    private IEnumerator ActiveDamagedState_co(float duration)
    {
        isDamaged = true;
        yield return new WaitForSeconds(duration);

        isDamaged = false;
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
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveY", 0f);
        animator.SetTrigger("Damaged");
    }

    // 넉백 종료시 속도 초기화 메소드
    private void HandleKnockbackEnd()
    {
        rb.linearVelocity = Vector3.zero;
    }
}
