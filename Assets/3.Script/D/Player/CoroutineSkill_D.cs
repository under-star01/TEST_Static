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
    public float speedBoostMultiplier = 1.5f;    // 시간 정지 중 추가 이동속도 배율

    private bool isTimeStopActive = false;       // 시간 정지 활성화 여부
    private float timeStopCooldownTimer = 0f;    // 쿨타임 타이머
    private float originalTimeScale = 1f;        // 원래 타임스케일
    private float originalFixedDeltaTime;        // 원래 물리 업데이트 시간

    private PlayerMove_D playerMove;
    private Rigidbody playerRb;
    private Animator playerAnimator;         // 플레이어 애니메이터
    private float originalLookSensitivity;   // 원래 마우스 감도

    [Header("스킬 2 설정 - 분신 소환/위치 교환")]
    public float cloneCooldown = 30f;            // 쿨타임

    private bool hasClone = false;               // 분신이 소환되어 있는지
    private GameObject currentClone;             // 현재 분신 오브젝트
    private Vector3 clonePosition;               // 분신 위치
    private float cloneCooldownTimer = 0f;       // 쿨타임 타이머

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

        // 분신 쿨타임 업데이트
        if (cloneCooldownTimer > 0)
        {
            cloneCooldownTimer -= Time.deltaTime;

            if (cloneCooldownTimer <= 0)
            {
                Debug.Log("분신 스킬 사용 가능!");
            }
        }
    }

    // 스킬 1 : 시간 정지

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

        Debug.Log("시간 정지 발동");

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

        Debug.Log("시간 정지 종료");

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

            // 이동 속도 보정 + 추가 속도 버프
            playerMove.moveSpeed *= compensation * speedBoostMultiplier;

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

            // 이동 속도 복구 (보정 + 버프 모두 제거)
            playerMove.moveSpeed /= (compensation * speedBoostMultiplier);

            // 마우스 감도 복구
            playerMove.lookSensitivity = originalLookSensitivity;
        }
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






















    //   스킬 2 : 분신 소환/위치 교환  

    public void OnSkill2()
    {
        // 쿨타임 중이면 사용 불가
        if (cloneCooldownTimer > 0)
        {
            Debug.Log($"분신 스킬 쿨타임 중! (남은 시간: {cloneCooldownTimer:F1}초)");
            return;
        }

        // 분신이 없으면 소환
        if (!hasClone)
        {
            SpawnClone();
        }
        // 분신이 있으면 위치 교환
        else
        {
            TeleportToClone();
        }
    }

    // 분신 소환
    private void SpawnClone()
    {
        // 현재 위치에 분신 생성
        clonePosition = transform.position;
        currentClone = CreateDefaultClone();
        
        hasClone = true;
        Debug.Log("분신 소환 완료! 다시 사용하면 분신 위치로 이동합니다.");
    }

    // 분신 위치로 텔레포트
    private void TeleportToClone()
    {
        if (currentClone == null)
        {
            // 분신이 파괴되었으면 재생성
            hasClone = false;
            Debug.LogWarning("분신이 파괴되었습니다. 다시 소환해주세요.");
            return;
        }

        // 현재 위치 저장
        Vector3 playerPos = transform.position;

        // 플레이어를 분신 위치로 이동
        transform.position = clonePosition;

        Debug.Log("분신 위치로 이동");

        // 분신 제거
        Destroy(currentClone);
        currentClone = null;
        hasClone = false;

        // 쿨타임 시작
        cloneCooldownTimer = cloneCooldown;
        Debug.Log($"분신 스킬 쿨타임 시작: {cloneCooldown}초");
    }

    // 기본 분신 생성
    private GameObject CreateDefaultClone()
    {
        GameObject clone = new GameObject("Clone");
        clone.transform.position = clonePosition;
        clone.transform.rotation = transform.rotation;

        // 플레이어의 렌더러 복사
        Renderer[] playerRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in playerRenderers)
        {
            // SkinnedMeshRenderer 복사
            SkinnedMeshRenderer originalSMR = (SkinnedMeshRenderer)rend;
            GameObject meshObj = new GameObject(rend.gameObject.name);
            meshObj.transform.SetParent(clone.transform);
            meshObj.transform.localPosition = rend.transform.localPosition;
            meshObj.transform.localRotation = rend.transform.localRotation;
            meshObj.transform.localScale = rend.transform.localScale;

            MeshRenderer cloneMR = meshObj.AddComponent<MeshRenderer>();
            MeshFilter cloneMF = meshObj.AddComponent<MeshFilter>();

            // 메시 복사
            Mesh mesh = new Mesh();
            originalSMR.BakeMesh(mesh);
            cloneMF.mesh = mesh;

            // 머테리얼 복사
            cloneMR.materials = originalSMR.materials;

        }


        return clone;
    }

    // 분신 쿨타임 반환 (UI 표시용)
    public float GetCloneCooldown()
    {
        return cloneCooldownTimer;
    }

    // 분신 스킬 사용 가능 여부 (UI 표시용)
    public bool IsCloneAvailable()
    {
        return cloneCooldownTimer <= 0;
    }

    // 분신이 소환되어 있는지 (UI 표시용)
    public bool HasClone()
    {
        return hasClone;
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
                playerMove.moveSpeed /= (compensation * speedBoostMultiplier);
                playerMove.lookSensitivity = originalLookSensitivity;
            }
        }

        // 분신 정리
        if (currentClone != null)
        {
            Destroy(currentClone);
        }
    }
}