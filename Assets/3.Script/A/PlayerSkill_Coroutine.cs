using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill_Coroutine : MonoBehaviour
{
    [Header("시간 정지 스킬 설정")]
    public float timeStopDuration = 3f;          // 시간 정지 지속 시간
    public float timeStopCooldown = 15f;         // 쿨타임
    public float slowMotionScale = 0.05f;        // 느려지는 정도 (0에 가까울수록 느림)
    public float speedBoostMultiplier = 1.5f;    // 시간 정지 중 추가 이동속도 배율

    private bool isTimeStopActive = false;       // 시간 정지 활성화 여부
    private float timeStopCooldownTimer = 0f;    // 쿨타임 타이머
    private float originalTimeScale = 1f;        // 원래 타임스케일
    private float originalFixedDeltaTime;        // 원래 물리 업데이트 시간

    private PlayerMove_A playerMove;
    private Rigidbody rb;
    private float mouseSensitivity_ori; // 원래 마우스 감도

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out playerMove);
    }

    // 스킬 1 : 시간 정지

    //public void OnTimeStopSkill()
    //{
    //    // 이미 시간 정지 중이면 사용 불가
    //    if (isTimeStopActive)
    //    {
    //        Debug.Log("이미 시간 정지 중입니다!");
    //        return;
    //    }

    //    // 쿨타임 중이면 사용 불가
    //    if (timeStopCooldownTimer > 0)
    //    {
    //        Debug.Log($"시간 정지 쿨타임 중! (남은 시간: {timeStopCooldownTimer:F1}초)");
    //        return;
    //    }

    //    // 시간 정지 시작
    //    StartCoroutine(TimeStopSequence());
    //}

    //private IEnumerator TimeStopSequence()
    //{
    //    isTimeStopActive = true;

    //    Debug.Log("시간 정지 발동");

    //    // 시간 정지 시작
    //    ActivateTimeStop();

    //    // 실제 시간으로 대기 (unscaledTime 사용)
    //    float elapsed = 0f;
    //    while (elapsed < timeStopDuration)
    //    {
    //        elapsed += Time.unscaledDeltaTime;
    //        yield return null;
    //    }

    //    // 시간 정지 해제
    //    DeactivateTimeStop();

    //    Debug.Log("시간 정지 종료");

    //    isTimeStopActive = false;

    //    // 쿨타임 시작
    //    timeStopCooldownTimer = timeStopCooldown;
    //    Debug.Log($"시간 정지 쿨타임 시작: {timeStopCooldown}초");
    //}

    //// 시간 정지 활성화
    //private void ActivateTimeStop()
    //{
    //    // 타임스케일 변경 (거의 정지)
    //    originalTimeScale = Time.timeScale;
    //    Time.timeScale = slowMotionScale;
    //    Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

    //    // 플레이어 애니메이터를 Unscaled Time으로 변경
    //    if (playerAnimator != null)
    //    {
    //        playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
    //    }

    //    // 플레이어 이동 속도 및 마우스 감도 보정 (플레이어는 정상 속도로)
    //    if (playerMove != null)
    //    {
    //        float compensation = 1f / slowMotionScale;

    //        // 이동 속도 보정 + 추가 속도 버프
    //        playerMove.moveSpeed *= compensation * speedBoostMultiplier;

    //        // 마우스 감도 보정
    //        originalLookSensitivity = playerMove.lookSensitivity;
    //        playerMove.lookSensitivity *= compensation;
    //    }
    //}

    //// 시간 정지 해제
    //private void DeactivateTimeStop()
    //{
    //    // 타임스케일 복구
    //    Time.timeScale = originalTimeScale;
    //    Time.fixedDeltaTime = originalFixedDeltaTime;

    //    // 플레이어 애니메이터를 Normal로 복구
    //    if (playerAnimator != null)
    //    {
    //        playerAnimator.updateMode = AnimatorUpdateMode.Normal;
    //    }

    //    // 플레이어 이동 속도 및 마우스 감도 복구
    //    if (playerMove != null)
    //    {
    //        float compensation = 1f / slowMotionScale;

    //        // 이동 속도 복구 (보정 + 버프 모두 제거)
    //        playerMove.moveSpeed /= (compensation * speedBoostMultiplier);

    //        // 마우스 감도 복구
    //        playerMove.lookSensitivity = originalLookSensitivity;
    //    }
    //}


    //// 현재 시간 정지 중인지 반환 (UI 표시용)
    //public bool IsTimeStopActive()
    //{
    //    return isTimeStopActive;
    //}

    //// 시간 정지 쿨타임 반환 (UI 표시용)
    //public float GetTimeStopCooldown()
    //{
    //    return timeStopCooldownTimer;
    //}

    //// 시간 정지 사용 가능 여부 (UI 표시용)
    //public bool IsTimeStopAvailable()
    //{
    //    return !isTimeStopActive && timeStopCooldownTimer <= 0;
    //}
}
