using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill_Coroutine : MonoBehaviour
{
    [Header("시간 정지 스킬 설정")]
    [SerializeField] private bool canShiftSkill = true;
    [SerializeField] private bool canSpaceSkill = true;

    public float timeStopCooldown = 10f;         // 쿨타임
    public float timeStopDuration = 4f;          // 시간 정지 지속 시간
    public float slowMotionScale = 0.05f;        // 느려지는 정도 (0에 가까울수록 느림)
    public float speedBoostMultiplier = 1.5f;    // 시간 정지 중 추가 이동속도 배율
    private float originalTimeScale = 1f;        // 기존 타임스케일
    private float originalFixedDeltaTime = 0.02f; // 기존 Fixed Time 타임 스케일

    [Header("분신 소환/위치 교환")]
    public float cloneCooldown = 10f;            // 쿨타임
    private bool hasClone = false;               // 분신이 소환되어 있는지
    private GameObject currentClone;             // 현재 분신 오브젝트
    private Vector3 clonePosition;               // 분신 위치

    [SerializeField] private GameObject StopEffectPrefab; //정지이펙트
    [SerializeField] private GameObject CloneEffectPrefab; //분신이펙트

    private PlayerMove_A playerMove;
    private Rigidbody rb;
    private float mouseSensitivity_ori; // 원래 마우스 감도
    private bool isShiftActive = false;
    private bool isSpaceActive = false;

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
        if (!canSpaceSkill || isShiftActive) return; // 쿨타임시 or 연속 입력 방지

        Debug.Log("Space 스킬 사용!");
        canSpaceSkill = false;
        isSpaceActive = true;

        AudioManager.Instance.PlaySlowSFX();
        if (StopEffectPrefab != null)
        {
            Instantiate(StopEffectPrefab, transform.position, Quaternion.identity);
        }

        // 시간 정지 메소드 실행
        StartCoroutine(Skill_WaitForSeconds_co());
        GameManager.Instance.ChangeSkillUIColor(4, true);
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
        isSpaceActive = false;
        GameManager.Instance.ChangeSkillUIColor(4, false);
        GameManager.Instance.SkillUIUpdate(4, timeStopCooldown);
    }

    // 시간 정지 활성화
    private void ActivateTimeStop()
    {
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
        Debug.Log("시간 정지 종료");
    }

    private IEnumerator TimeStopCool_co()
    {
        yield return new WaitForSeconds(timeStopCooldown);

        canSpaceSkill = true;
    }

    // 스킬 2: 잔상 생성
    public void UseSkill_Shift()
    {
        if (!canShiftSkill || isSpaceActive) return; // 쿨타임시 or 연속 입력 방지
        Debug.Log("Shift 스킬 사용!");
        isShiftActive = true;
        GameManager.Instance.ChangeSkillUIColor(5, true);
        StartCoroutine(Skill_YieldReturn_co());
    }

    private IEnumerator Skill_YieldReturn_co()
    {
        // 상태 제한
        rb.linearVelocity = Vector3.zero;
        playerMove.isMoveLocked = true;

        // 애니메이션 파라미터 초기화 및 실행
        playerMove.animator.SetFloat("MoveX", 0f);
        playerMove.animator.SetFloat("MoveY", 0f);
        playerMove.animator.SetTrigger("Skill_Shift");

        // 분신이 없으면 소환
        if (!hasClone)
        {
            AudioManager.Instance.PlayCloneSFX(); //사운드
            if (CloneEffectPrefab != null)
            {
                Instantiate(CloneEffectPrefab, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.4f);
            SpawnClone();
        }
        // 분신이 있으면 위치 교환
        else
        {
            if (CloneEffectPrefab != null)
            {
                Instantiate(CloneEffectPrefab, transform.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(0.4f);
            AudioManager.Instance.PlayYieldReturnSFX();
            TeleportToClone();
        }

        // 입력 제한 및 스킬 제한 복구
        playerMove.isMoveLocked = false;
    }

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
        canShiftSkill = false;

        // 쿨타임 시작
        StartCoroutine(ChangeCloneCool_co());
        isShiftActive = false;
        GameManager.Instance.ChangeSkillUIColor(5, false);
        GameManager.Instance.SkillUIUpdate(5, cloneCooldown);
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

    private IEnumerator ChangeCloneCool_co()
    {
        yield return new WaitForSeconds(cloneCooldown);

        canShiftSkill = true;
    }
}
