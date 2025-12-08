using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill_Transform : MonoBehaviour
{
    [SerializeField] private Collider mapCollider; // 맵 범위 체크용

    [Header("텔레포트 스킬 설정")]
    public float teleportDistance = 6f;          // 텔레포트 거리
    public float cooldownTime = 6f;              // 쿨타임
    public int maxStacks = 2;                    // 최대 스택 수

    [Header("텔레포트 효과 설정")]
    public float fadeOutTime = 0.2f;             // 사라지는 시간
    public float invisibleTime = 0.1f;           // 사라진 상태 유지 시간
    public float fadeInTime = 0.2f;              // 나타나는 시간

    private int currentStacks = 2;               // 현재 스택
    private List<float> cooldownTimers = new List<float>(); // 각 스택별 쿨타임 타이머
    private bool isTeleporting = false;          // 텔레포트 중 여부

    [Header("작아지기 스킬 설정")]
    public float shrinkScale = 0.5f;
    public float shrinkDuration = 5f;
    public float scaleTransitionTime = 0.5f;     // 크기 변화 시간
    public float shrinkCooldownTime = 20f;       // 작아지기 쿨타임
    private bool isShrunken = false;
    private float shrinkCooldownTimer = 0f;      // 쿨타임 타이머

    private Vector3 originalScale;

    private PlayerMove_A playerMove;

    private void Awake()
    {
        TryGetComponent(out playerMove);
    }

    // Shift: 일정 시간 완전 무적 스킬
    public void UseSkill_Space()
    {
        // 이미 텔레포트 중이거나 스택이 없으면 사용 불가
        if (isTeleporting)
        {
            return;
        }
        if (currentStacks <= 0)
        {
            return;
        }
        StartCoroutine(Skill_Teleport());
    }

    private IEnumerator Skill_Teleport()
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

        yield return new WaitForSeconds(0.5f);

        // 텔레포트 실행
        transform.position = targetPos;

        // 스택 소모
        currentStacks--;
        cooldownTimers.Add(cooldownTime);

        isTeleporting = false;
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

    public void UseSkill_Shift()
    {
        // 이미 작아진 상태면 사용 불가
        if (isShrunken)
        {
            return;
        }

        // 쿨타임 중이면 사용 불가
        if (shrinkCooldownTimer > 0)
        {
            return;
        }

        StartCoroutine(Skill_Scale());
    }

    private IEnumerator Skill_Scale()
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
