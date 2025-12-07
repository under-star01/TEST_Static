using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoroutineSkill : MonoBehaviour
{
    [Header("시간 정지 스킬 설정")]
    public float timeStopDuration = 3f;          // 시간 정지 지속 시간
    public float timeStopCooldown = 15f;         // 쿨타임
    public float slowMotionScale = 0.05f;        // 느려지는 정도 (0에 가까울수록 느림)

    private bool isTimeStopActive = false;       // 시간 정지 활성화 여부
    private float timeStopCooldownTimer = 0f;    // 쿨타임 타이머
    private float originalTimeScale = 1f;        // 원래 타임스케일
    private float originalFixedDeltaTime;        // 원래 물리 업데이트 시간

    private PlayerMove_D playerMove;
    private Rigidbody playerRb;
    private Animator playerAnimator;         // 플레이어 애니메이터
    private float originalLookSensitivity;   // 원래 마우스 감도

    [Header("스킬 2 설정")]
    public float skill2Duration = 5f;            // 스킬 2 지속 시간
    public float skill2Cooldown = 20f;           // 스킬 2 쿨타임
    private bool isSkill2Active = false;
    private float skill2CooldownTimer = 0f;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove_D>();
        playerRb = GetComponent<Rigidbody>();
        playerAnimator = GetComponentInChildren<Animator>();

        // 원래 물리 시간 저장
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        // 시간 정지 쿨타임 업데이트 (실제 시간 기준)
        if (timeStopCooldownTimer > 0)
        {
            timeStopCooldownTimer -= Time.unscaledDeltaTime;

            if (timeStopCooldownTimer <= 0)
            {
                Debug.Log("시간 정지 스킬 사용 가능!");
            }
        }

        // 스킬 2 쿨타임 업데이트
        if (skill2CooldownTimer > 0)
        {
            skill2CooldownTimer -= Time.deltaTime;

            if (skill2CooldownTimer <= 0)
            {
                Debug.Log("스킬 2 사용 가능!");
            }
        }
    }

    // ----------------------------- //
    //     스킬 1 : 시간 정지         //
    // ----------------------------- //

    public void OnTimeStopSkill()
    {
        // 이미 시간 정지 중이면 사용 불가
        if (isTimeStopActive)
        {
            Debug.Log("이미 시간 정지 중입니다!");
            return;
        }

        // 쿨타임 중이면 사용 불가
        if (timeStopCooldownTimer > 0)
        {
            Debug.Log($"시간 정지 쿨타임 중! (남은 시간: {timeStopCooldownTimer:F1}초)");
            return;
        }

        // 시간 정지 시작
        StartCoroutine(TimeStopSequence());
    }

    private IEnumerator TimeStopSequence()
    {
        isTimeStopActive = true;

        Debug.Log("시간 정지 발동!");

        // 시간 정지 시작
        ActivateTimeStop();

        // 실제 시간으로 대기 (unscaledTime 사용)
        float elapsed = 0f;
        while (elapsed < timeStopDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 시간 정지 해제
        DeactivateTimeStop();

        Debug.Log("시간 정지 종료!");

        isTimeStopActive = false;

        // 쿨타임 시작
        timeStopCooldownTimer = timeStopCooldown;
        Debug.Log($"시간 정지 쿨타임 시작: {timeStopCooldown}초");
    }

    // 시간 정지 활성화
    private void ActivateTimeStop()
    {
        // 타임스케일 변경 (거의 정지)
        originalTimeScale = Time.timeScale;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        // 플레이어 애니메이터를 Unscaled Time으로 변경
        if (playerAnimator != null)
        {
            playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        // 플레이어 이동 속도 및 마우스 감도 보정 (플레이어는 정상 속도로)
        if (playerMove != null)
        {
            float compensation = 1f / slowMotionScale;

            // 이동 속도 보정
            playerMove.moveSpeed *= compensation;

            // 마우스 감도 보정
            originalLookSensitivity = playerMove.lookSensitivity;
            playerMove.lookSensitivity *= compensation;
        }
    }

    // 시간 정지 해제
    private void DeactivateTimeStop()
    {
        // 타임스케일 복구
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // 플레이어 애니메이터를 Normal로 복구
        if (playerAnimator != null)
        {
            playerAnimator.updateMode = AnimatorUpdateMode.Normal;
        }

        // 플레이어 이동 속도 및 마우스 감도 복구
        if (playerMove != null)
        {
            float compensation = 1f / slowMotionScale;

            // 이동 속도 복구
            playerMove.moveSpeed /= compensation;

            // 마우스 감도 복구
            playerMove.lookSensitivity = originalLookSensitivity;
        }
    }


    // 플레이어의 렌더러인지 체크
    private bool IsPlayerRenderer(Renderer rend)
    {
        Transform current = rend.transform;
        while (current != null)
        {
            if (current == transform)
                return true;
            current = current.parent;
        }
        return false;
    }

    // 현재 시간 정지 중인지 반환 (UI 표시용)
    public bool IsTimeStopActive()
    {
        return isTimeStopActive;
    }

    // 시간 정지 쿨타임 반환 (UI 표시용)
    public float GetTimeStopCooldown()
    {
        return timeStopCooldownTimer;
    }

    // 시간 정지 사용 가능 여부 (UI 표시용)
    public bool IsTimeStopAvailable()
    {
        return !isTimeStopActive && timeStopCooldownTimer <= 0;
    }

    // ----------------------------- //
    //       스킬 2 : 임시           //
    // ----------------------------- //

    public void OnSkill2()
    {
        // 이미 스킬 2 사용 중이면 사용 불가
        if (isSkill2Active)
        {
            Debug.Log("이미 스킬 2 사용 중입니다!");
            return;
        }

        // 쿨타임 중이면 사용 불가
        if (skill2CooldownTimer > 0)
        {
            Debug.Log($"스킬 2 쿨타임 중! (남은 시간: {skill2CooldownTimer:F1}초)");
            return;
        }

        Debug.Log("스킬 2 사용!");
        StartCoroutine(Skill2Routine());
    }

    private IEnumerator Skill2Routine()
    {
        isSkill2Active = true;

        // 여기에 스킬 2 효과 추가
        // 예: 이동속도 증가, 무적, 투명화 등

        yield return new WaitForSeconds(skill2Duration);

        isSkill2Active = false;

        // 쿨타임 시작
        skill2CooldownTimer = skill2Cooldown;
        Debug.Log($"스킬 2 쿨타임 시작: {skill2Cooldown}초");
    }

    // 스킬 2 쿨타임 반환 (UI 표시용)
    public float GetSkill2Cooldown()
    {
        return skill2CooldownTimer;
    }

    // 스킬 2 사용 가능 여부 (UI 표시용)
    public bool IsSkill2Available()
    {
        return !isSkill2Active && skill2CooldownTimer <= 0;
    }

    private void OnDestroy()
    {
        // 스크립트가 파괴될 때 타임스케일 복구
        if (isTimeStopActive)
        {
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;

            // 애니메이터 복구
            if (playerAnimator != null)
            {
                playerAnimator.updateMode = AnimatorUpdateMode.Normal;
            }

            // 이동 속도 및 마우스 감도 복구
            if (playerMove != null)
            {
                float compensation = 1f / slowMotionScale;
                playerMove.moveSpeed /= compensation;
                playerMove.lookSensitivity = originalLookSensitivity;
            }
        }
    }
}




















