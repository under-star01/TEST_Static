using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill_Coroutine : MonoBehaviour
{
    [Header("시간 정지 스킬 설정")]
    [SerializeField] private bool isShiftRunning = false;
    [SerializeField] private bool isSpaceRunning = false;
    [SerializeField] private bool canShiftSkill = true;

    public float timeStopDuration = 3f;          // 시간 정지 지속 시간
    public float timeStopCooldown = 1f;         // 쿨타임
    public float slowMotionScale = 0.05f;        // 느려지는 정도 (0에 가까울수록 느림)
    public float speedBoostMultiplier = 1.5f;    // 시간 정지 중 추가 이동속도 배율
    private float originalTimeScale = 1f;        // 기존 타임스케일
    private float originalFixedDeltaTime = 0.02f; // 기존 Fixed Time 타임 스케일

    [Header("분신 소환/위치 교환")]
    public float cloneCooldown = 30f;            // 쿨타임

    private bool hasClone = false;               // 분신이 소환되어 있는지
    private GameObject currentClone;             // 현재 분신 오브젝트
    private Vector3 clonePosition;               // 분신 위치
    private float cloneCooldownTimer = 0f;       // 쿨타임 타이머


    private PlayerMove_A playerMove;
    private Rigidbody rb;
    private float mouseSensitivity_ori; // 원래 마우스 감도

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out playerMove);

        // 시간 간격 설정
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    // 스킬 1: 시간 정지
    public void UseSkill_Space()
    {
        if (!canShiftSkill || isSpaceRunning) return; // 쿨타임시 or 연속 입력 방지

        Debug.Log("Space 스킬 사용!");
        // 시간 정지 메소드 실행
        StartCoroutine(Skill_WaitForSeconds_co());
    }

    private IEnumerator Skill_WaitForSeconds_co()
    {
        // 시간 정지 시작
        ActivateTimeStop();
        yield return new WaitForSecondsRealtime(timeStopDuration); // 실제 시간으로 계산

        // 시간 정지 해제
        DeactivateTimeStop();


        // 쿨타임 시작
        StartCoroutine(TimeStopCool_co());
    }

    // 시간 정지 활성화
    private void ActivateTimeStop()
    {
        isSpaceRunning = true;

        // 타임스케일 변경 (거의 정지)
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * slowMotionScale;

        // 플레이어 애니메이터를 Unscaled Time으로 변경
        if (playerMove.animator != null)
        {
            playerMove.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        // 플레이어 이동 속도 및 마우스 감도 보정 (플레이어는 정상 속도로)
        if (playerMove != null)
        {
            float compensation = 1f / slowMotionScale;

            // 이동 속도 보정 + 추가 속도 버프
            playerMove.moveSpeed *= compensation * speedBoostMultiplier;

            // 마우스 감도 보정
            mouseSensitivity_ori = playerMove.lookSensitivity;
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
        if (playerMove.animator != null)
        {
            playerMove.animator.updateMode = AnimatorUpdateMode.Normal;
        }

        // 플레이어 이동 속도 및 마우스 감도 보정 (플레이어는 정상 속도로)
        if (playerMove != null)
        {
            float compensation = 1f / slowMotionScale;

            // 이동 속도 복구 (보정 + 버프 모두 제거)
            playerMove.moveSpeed /= (compensation * speedBoostMultiplier);

            // 마우스 감도 복구
            playerMove.lookSensitivity = mouseSensitivity_ori;
        }
        isSpaceRunning = false;
        Debug.Log("시간 정지 종료");
    }

    private IEnumerator TimeStopCool_co()
    {
        canShiftSkill = false;
        yield return new WaitForSeconds(timeStopCooldown);

        canShiftSkill = true;
    }




}
