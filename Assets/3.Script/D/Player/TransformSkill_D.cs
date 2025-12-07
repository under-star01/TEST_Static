using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransformSkill_D : MonoBehaviour
{
    [Header("텔레포트 스킬 설정")]
    public float teleportDistance = 10f;          // 텔레포트 거리
    public float cooldownTime = 20f;              // 쿨타임
    public int maxStacks = 2;                     // 최대 스택 수
    [SerializeField] private Collider mapCollider; // 맵 범위 체크용

    private int currentStacks = 2;                // 현재 스택
    private List<float> cooldownTimers = new List<float>(); // 각 스택별 쿨타임

    private PlayerMove_D playerMove;

    [Header("작아지기 스킬 설정")]
    public float shrinkScale = 0.5f;
    public float shrinkDuration = 5f;
    private bool isShrunken = false;

    private Vector3 originalScale;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove_D>();
        originalScale = transform.localScale;

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
        // 스택이 없으면 사용 불가
        if (currentStacks <= 0)
        {
            Debug.Log($"텔레포트 쿨타임 중! (남은 시간: {GetMinCooldown():F1}초)");
            return;
        }

        // 전방으로 텔레포트
        Vector3 targetPos = transform.position + transform.forward * teleportDistance;
        targetPos.y = transform.position.y; // 높이 유지

        // 맵 범위 체크
        if (IsInsideMap(targetPos))
        {
            transform.position = targetPos;

            // 스택 소모
            currentStacks--;
            cooldownTimers.Add(cooldownTime);

            Debug.Log($"텔레포트 완료! 남은 스택: {currentStacks}/{maxStacks}");
        }
        else
        {
            Debug.Log("맵 밖으로는 텔레포트할 수 없습니다.");
        }
    }

    // 맵 범위 체크 메소드
    private bool IsInsideMap(Vector3 position)
    {
        if (mapCollider == null)
            return true; // 맵 콜라이더가 없으면 제한 없음

        Bounds bounds = mapCollider.bounds;
        return position.x >= bounds.min.x && position.x <= bounds.max.x &&
               position.z >= bounds.min.z && position.z <= bounds.max.z;
    }

    // 가장 짧은 쿨타임 반환
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

    // TransformInput_D에서 호출됨
    public void OnShrinkSkill()
    {
        if (isShrunken) return;

        Debug.Log("작아지기 스킬 활성화!");
        StartCoroutine(ShrinkRoutine());
    }

    private IEnumerator ShrinkRoutine()
    {
        isShrunken = true;

        // 작아짐
        transform.localScale = originalScale * shrinkScale;
        playerMove.moveSpeed *= 1.2f; // 이동속도 살짝 증가

        yield return new WaitForSeconds(shrinkDuration);

        // 원래 크기로 복구
        transform.localScale = originalScale;
        playerMove.moveSpeed /= 1.2f;

        isShrunken = false;
    }
}






