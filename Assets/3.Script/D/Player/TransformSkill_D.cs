using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransformSkill_D : MonoBehaviour
{
    [Header("텔레포트")]
    public float teleportDistance = 5f;          // 텔레포트 거리
    public float cooldownTime = 6f;              // 쿨타임
    public int maxStacks = 2;                    // 최대 스택 수
    [SerializeField] private Collider mapCollider; // 맵 범위 체크용

    private int currentStacks = 2;               // 현재 스택
    private List<float> cooldownTimers = new List<float>(); // 각 스택별 쿨타임
    private bool isTeleporting = false;          // 텔레포트 중 플래그

    private PlayerMove_D playerMove;
    private Renderer[] renderers;                // 모든 렌더러들
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>(); // 원래 색상 저장

    [Header("작아지기")]
    public float shrinkScale = 0.5f;
    public float shrinkDuration = 5f;
    public float scaleTransitionTime = 0.5f;     // 크기 변화 시간
    public float shrinkCooldownTime = 20f;       // 작아지기 쿨타임
    private bool isShrunken = false;
    private float shrinkCooldownTimer = 0f;      // 쿨타임 타이머

    private Vector3 originalScale;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove_D>();
        originalScale = transform.localScale;

        // 모든 렌더러 가져오기 (자식 포함)
        renderers = GetComponentsInChildren<Renderer>();

        // 원래 색상 저장
        foreach (Renderer rend in renderers)
        {
            if (rend is SkinnedMeshRenderer || rend is MeshRenderer)
            {
                originalColors[rend] = rend.material.color;
            }
            else if (rend is SpriteRenderer)
            {
                originalColors[rend] = ((SpriteRenderer)rend).color;
            }
        }

        // 시작 시 최대 스택
        currentStacks = maxStacks;
    }

    private void Update()
    {
        // 텔레포트 쿨타임 업데이트
        for (int i = cooldownTimers.Count - 1; i >= 0; i--)
        {
            cooldownTimers[i] -= Time.deltaTime;

            if (cooldownTimers[i] <= 0)
            {
                // 쿨타임 완료, 스택 회복
                currentStacks++;
                cooldownTimers.RemoveAt(i);
                Debug.Log($"텔레포트 스택 충전 현재 스택: {currentStacks}/{maxStacks}");
            }
        }

        // 작아지기 쿨타임 업데이트
        if (shrinkCooldownTimer > 0)
        {
            shrinkCooldownTimer -= Time.deltaTime;

            if (shrinkCooldownTimer <= 0)
            {
                Debug.Log("작아지기 스킬 사용 가능");
            }
        }
    }

    //  스킬 1 : 순간이동 

    public void OnTeleportStart()
    {
        // 이미 텔레포트 중이거나 스택이 없으면 사용 불가
        if (isTeleporting)
        {
            Debug.Log("이미 텔레포트 중");
            return;
        }

        if (currentStacks <= 0)
        {
            Debug.Log($"텔레포트 쿨타임 (남은 시간: {GetMinCooldown():F1}초)");
            return;
        }

        // 텔레포트 코루틴 시작
        StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        // 현재 이동 방향 계산
        Vector3 moveDirection = GetCurrentMoveDirection();
        if (moveDirection == Vector3.zero)
        {
            moveDirection = transform.forward;
        }

        // 텔레포트 목표 위치 계산
        Vector3 targetPos = transform.position + moveDirection * teleportDistance;
        targetPos.y = transform.position.y; // 높이 유지

        // 맵 밖이면 취소
        if (!IsInsideMap(targetPos))
        {
            Debug.Log("맵 밖으로는 텔레포트할 수 없음");

            isTeleporting = false;
            yield break;
        }

        // 텔레포트 실행
        transform.position = targetPos;

        // 스택 소모
        currentStacks--;
        cooldownTimers.Add(cooldownTime);
        Debug.Log($"텔레포트! 남은 스택: {currentStacks}/{maxStacks}");

        // 텔레포트 완료
        isTeleporting = false;
        Debug.Log("텔레포트 완료");
    }


    // 현재 이동 방향 계산
    private Vector3 GetCurrentMoveDirection()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0; // y축 제거

            if (velocity.magnitude > 0.1f)
            {
                return velocity.normalized;
            }
        }

        return Vector3.zero;
    }

    // 맵 범위 체크
    private bool IsInsideMap(Vector3 position)
    {
        if (mapCollider == null)
            return true; // 맵 콜라이더가 없으면 제한 없음

        Bounds bounds = mapCollider.bounds;
        return position.x >= bounds.min.x && position.x <= bounds.max.x &&
               position.z >= bounds.min.z && position.z <= bounds.max.z;
    }

    // 쿨타임 반환
    private float GetMinCooldown()
    {
        if (cooldownTimers.Count == 0)
            return 0f;

        float min = cooldownTimers[0];
        foreach (float time in cooldownTimers)
        {
            if (time < min)
                min = time;
        }
        return min;
    }

    // 현재 스택 수 반환 (UI 표시용)
    public int GetCurrentStacks()
    {
        return currentStacks;
    }

    // 다음 스택 충전까지 남은 시간 (UI 표시용)
    public float GetNextStackCooldown()
    {
        return GetMinCooldown();
    }

    // 작아지기 스킬 쿨타임 반환 (UI 표시용)
    public float GetShrinkCooldown()
    {
        return shrinkCooldownTimer;
    }

    // 작아지기 스킬 사용 가능 여부 (UI 표시용)
    public bool IsShrinkAvailable()
    {
        return !isShrunken && shrinkCooldownTimer <= 0;
    }

    //     스킬 2 : 작아지기
    public void OnShrinkSkill()
    {
        // 이미 작아진 상태면 사용 불가
        if (isShrunken)
        {
            Debug.Log("이미 작아지기 스킬 사용 중");
            return;
        }

        // 쿨타임 중이면 사용 불가
        if (shrinkCooldownTimer > 0)
        {
            Debug.Log($"작아지기 스킬 쿨타임 (남은 시간: {shrinkCooldownTimer:F1}초)");
            return;
        }

        Debug.Log("작아지기 스킬 씀");
        StartCoroutine(ShrinkRoutine());
    }

    private IEnumerator ShrinkRoutine()
    {
        isShrunken = true;

        // 부드럽게 작아지기
        Vector3 targetScale = originalScale * shrinkScale;
        yield return StartCoroutine(ScaleTo(targetScale, scaleTransitionTime));

        // 이동속도 증가
        playerMove.moveSpeed *= 2f;

        // 작아진 상태로 유지
        yield return new WaitForSeconds(shrinkDuration);

        // 이동속도 원래대로
        playerMove.moveSpeed /= 2f;

        // 부드럽게 커지기
        yield return StartCoroutine(ScaleTo(originalScale, scaleTransitionTime));

        isShrunken = false;

        // 쿨타임 시작
        shrinkCooldownTimer = shrinkCooldownTime;
        Debug.Log($"작아지기 스킬 쿨타임 시작: {shrinkCooldownTime}초");
    }

    // 부드럽게 크기 변경하는 코루틴
    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // EaseInOutQuad 곡선 사용 (부드러운 가속/감속)
            t = t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}