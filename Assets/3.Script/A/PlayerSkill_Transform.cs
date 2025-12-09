using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill_Transform : MonoBehaviour
{
    [SerializeField] private Collider mapCollider; // 맵 범위 체크용

    [Header("텔레포트 스킬 설정")]
    public float teleportDistance = 6f;          // 텔레포트 거리
    public float TeleportcooldownTime = 6f;              // 쿨타임
    [SerializeField] private bool canSpaceSkill = true;
    [SerializeField] private GameObject teleportEffectPrefab; //텔포이펙트
    [SerializeField] private GameObject ScailEffectPrefab; //스케일이펙트


    [Header("작아지기 스킬 설정")]
    public float shrinkScale = 0.2f;
    public float shrinkDuration = 5f;
    public float scaleTransitionTime = 0.5f;     // 크기 변화 시간
    public float shrinkCooldownTime = 10f;       // 작아지기 쿨타임
    private bool canShiftSkill = true;

    private Vector3 originalScale;

    private PlayerMove_A playerMove;

    private void Awake()
    {
        TryGetComponent(out playerMove);
        originalScale = transform.localScale;
    }

    // Shift: 일정 시간 완전 무적 스킬
    public void UseSkill_Space()
    {
        // 쿨타임 일때 돌아가
        if (!canSpaceSkill)
        {
            return;
        }
        canSpaceSkill = false;
        StartCoroutine(Skill_Teleport());
    }

    private IEnumerator Skill_Teleport()
    {
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        }


        // 현재 이동 방향 계산
        Vector3 moveDirection = GetCurrentMoveDirection();
        if (moveDirection == Vector3.zero)
        {
            moveDirection = transform.forward;
        }


        // 텔레포트 목표 위치 계산
        Vector3 targetPos = transform.position + moveDirection * teleportDistance;
        targetPos.y = transform.position.y; // 높이 유지

        // 텔레포트 실행
        yield return new WaitForSeconds(0.2f);

        transform.position = targetPos;

        AudioManager.Instance.PlayTeleportSFX(); //소리 재생

        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        }

        StartCoroutine(TeleportCool_co());
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

    private IEnumerator TeleportCool_co()
    {
        yield return new WaitForSeconds(TeleportcooldownTime);

        canSpaceSkill = true;
    }


    public void UseSkill_Shift()
    {
        // 쿨타임 일때 돌아가
        if (!canShiftSkill)
        {
            return;
        }
        canShiftSkill = false;
        StartCoroutine(Skill_Scale());
    }

    private IEnumerator ScaleCool_co()
    {
        yield return new WaitForSeconds(shrinkCooldownTime);

        canShiftSkill = true;
    }

    private IEnumerator Skill_Scale()
    {
        AudioManager.Instance.PlayScailDownSFX();//작아지기 사운드

        if (ScailEffectPrefab != null)
        {
            Instantiate(ScailEffectPrefab, transform.position, Quaternion.identity);
        }

        // 부드럽게 작아지기
        Vector3 targetScale = originalScale * shrinkScale;
        yield return StartCoroutine(ScaleTo(targetScale, scaleTransitionTime));


        // 이동속도 증가
        playerMove.moveSpeed *= 1.7f;

        // 작아진 상태로 유지
        yield return new WaitForSeconds(shrinkDuration);

        // 이동속도 원래대로
        playerMove.moveSpeed /= 1.7f;

        AudioManager.Instance.PlayScailReturnSFX(); // 돌아오기 사운드

        if (ScailEffectPrefab != null)
        {
            Instantiate(ScailEffectPrefab, transform.position, Quaternion.identity);
        }
        // 부드럽게 커지기
        yield return StartCoroutine(ScaleTo(originalScale, scaleTransitionTime));



        StartCoroutine(ScaleCool_co());
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
