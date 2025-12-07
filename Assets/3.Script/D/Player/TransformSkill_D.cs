using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransformSkill_D : MonoBehaviour
{
    [Header("텔레포트 스킬 설정")]
    public float teleportDistance = 5f;          // 텔레포트 거리
    public float cooldownTime = 6f;              // 쿨타임
    public int maxStacks = 2;                    // 최대 스택 수
    [SerializeField] private Collider mapCollider; // 맵 범위 체크용

    [Header("텔레포트 효과 설정")]
    public float fadeOutTime = 0.2f;             // 사라지는 시간
    public float invisibleTime = 0.1f;           // 사라진 상태 유지 시간
    public float fadeInTime = 0.2f;              // 나타나는 시간

    private int currentStacks = 2;               // 현재 스택
    private List<float> cooldownTimers = new List<float>(); // 각 스택별 쿨타임
    private bool isTeleporting = false;          // 텔레포트 중 플래그

    private PlayerMove_D playerMove;
    private Renderer[] renderers;                // 모든 렌더러들
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>(); // 원래 색상 저장

    [Header("작아지기 스킬 설정")]
    public float shrinkScale = 0.5f;
    public float shrinkDuration = 5f;
    public float scaleTransitionTime = 0.5f;     // 크기 변화 시간
    private bool isShrunken = false;

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
        // 쿨타임 업데이트
        for (int i = cooldownTimers.Count - 1; i >= 0; i--)
        {
            cooldownTimers[i] -= Time.deltaTime;

            if (cooldownTimers[i] <= 0)
            {
                // 쿨타임 완료, 스택 회복
                currentStacks++;
                cooldownTimers.RemoveAt(i);
                Debug.Log($"텔레포트 스택 충전! 현재 스택: {currentStacks}/{maxStacks}");
            }
        }
    }

    // ----------------------------- //
    //       스킬 1 : 순간이동        //
    // ----------------------------- //

    // TransformInput_D에서 호출됨
    public void OnTeleportStart()
    {
        // 이미 텔레포트 중이거나 스택이 없으면 사용 불가
        if (isTeleporting)
        {
            Debug.Log("이미 텔레포트 중입니다!");
            return;
        }

        if (currentStacks <= 0)
        {
            Debug.Log($"텔레포트 쿨타임 중! (남은 시간: {GetMinCooldown():F1}초)");
            return;
        }

        // 텔레포트 코루틴 시작
        StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        // 1. 페이드 아웃 (사라지기) - 이동 가능
        yield return StartCoroutine(FadeOut(fadeOutTime));

        // 2. 페이드 아웃 끝난 후 현재 이동 방향 계산
        Vector3 moveDirection = GetCurrentMoveDirection();
        if (moveDirection == Vector3.zero)
        {
            moveDirection = transform.forward;
        }

        // 3. 텔레포트 목표 위치 계산
        Vector3 targetPos = transform.position + moveDirection * teleportDistance;
        targetPos.y = transform.position.y; // 높이 유지

        // 맵 밖이면 취소하고 다시 나타나기
        if (!IsInsideMap(targetPos))
        {
            Debug.Log("맵 밖으로는 텔레포트할 수 없습니다.");
            yield return StartCoroutine(FadeIn(fadeInTime));
            isTeleporting = false;
            yield break;
        }

        // 4. 완전히 사라진 상태로 잠시 대기
        yield return new WaitForSeconds(invisibleTime);

        // 5. 실제 텔레포트 실행
        transform.position = targetPos;

        // 6. 스택 소모
        currentStacks--;
        cooldownTimers.Add(cooldownTime);
        Debug.Log($"텔레포트 완료! 남은 스택: {currentStacks}/{maxStacks}");

        // 7. 페이드 인 (나타나기)
        yield return StartCoroutine(FadeIn(fadeInTime));

        // 8. 텔레포트 완료
        isTeleporting = false;
        Debug.Log("텔레포트 시퀀스 완료");
    }

    // 페이드 아웃 (투명해지기)
    private IEnumerator FadeOut(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration); // 1 -> 0

            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f); // 완전 투명
    }

    // 페이드 인 (불투명해지기)
    private IEnumerator FadeIn(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / duration; // 0 -> 1

            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(1f); // 완전 불투명
    }

    // 모든 렌더러의 투명도 설정
    private void SetAlpha(float alpha)
    {
        foreach (Renderer rend in renderers)
        {
            if (originalColors.ContainsKey(rend))
            {
                Color newColor = originalColors[rend];
                newColor.a = alpha;

                if (rend is SkinnedMeshRenderer || rend is MeshRenderer)
                {
                    rend.material.color = newColor;
                }
                else if (rend is SpriteRenderer)
                {
                    ((SpriteRenderer)rend).color = newColor;
                }
            }
        }
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

    // ----------------------------- //
    //     스킬 2 : 작아지기         //
    // ----------------------------- //

    public void OnShrinkSkill()
    {
        if (isShrunken) return;

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