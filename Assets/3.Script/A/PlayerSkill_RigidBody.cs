using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill_RigidBody : MonoBehaviour
{
    [Header("Space 스킬 설정")]
    [SerializeField] private bool canShiftSkill = true;
    [SerializeField] private bool canSpaceSkill = true;

    [SerializeField] private float addForceCool = 10f;
    [SerializeField] private float isTriggerCool = 15f;
    [SerializeField] private float spaceRadius = 5f; // 스킬 범위
    [SerializeField] private LayerMask detectLayer; // 탐지 레이어

    [Header("범위 표시 오브젝트")]
    [SerializeField] private GameObject rangeIndicator; // 붉은 원 Mesh/Quad (미리 프리팹 or 자식으로 세팅)

    [SerializeField] private GameObject shinratenseiEffectPrefab; //신라천정이펙트
    [SerializeField] private GameObject invincibleEffectPrefab; //무적이펙트

    private PlayerMove_A playerMove;
    private Rigidbody rb;
    
    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out playerMove);
    }

    // Shift: 일정 시간 완전 무적 스킬
    public void UseSkill_Shift()
    {
        if (!canShiftSkill) return; // 연속 입력 방지

        Debug.Log("Shift 스킬 사용!");
        canShiftSkill = false;
        AudioManager.Instance.PlayIstriggerSFX(); //사운드
        if (invincibleEffectPrefab != null)
        {
            Instantiate(invincibleEffectPrefab, transform.position, Quaternion.identity);
        }
        StartCoroutine(Skill_IsTrigger(5f)); // 5초 동안 스킬 사용
    }


    // 스킬 사용 코루틴: IsTrigger
    private IEnumerator Skill_IsTrigger(float duration)
    {
        // 상태 제한
        rb.linearVelocity = Vector3.zero;
        playerMove.isMoveLocked = true;

        // 애니메이션 파라미터 초기화 및 실행
        playerMove.animator.SetFloat("MoveX", 0f);
        playerMove.animator.SetFloat("MoveY", 0f);
        playerMove.animator.SetTrigger("Skill_Shift");
        yield return new WaitForSeconds(0.7f);

        // 입력 제한 및 스킬 제한 복구
        playerMove.isMoveLocked = false;

        // 무적 레이어 적용
        gameObject.layer = LayerMask.NameToLayer("Invincibility");
        yield return new WaitForSeconds(duration);

        AudioManager.Instance.PlayIstriggerEndSFX();//사운드

        if (invincibleEffectPrefab != null)
        {
            Instantiate(invincibleEffectPrefab, transform.position, Quaternion.identity);
        }

        // 레이어 복구
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        // 쿨타임 시작
        StartCoroutine(IsTriggerCool_co());
    }

    // Space: 주변 방해물 날려버리기
    public void UseSkill_Space()
    {
        if (!canSpaceSkill) return; // 연속 입력 방지

        Debug.Log("Space 스킬 사용!");
        canSpaceSkill = false;

        AudioManager.Instance.PlayAddForceSFX();

        StartCoroutine(UseSpaceRoutine());
    }

    // Space 스킬 실행 코루틴 (주변 방해물 날려버리기)
    private IEnumerator UseSpaceRoutine()
    {
        // 상태 제한
        rb.linearVelocity = Vector3.zero;
        playerMove.isMoveLocked = true;

        // 애니메이션 파라미터 초기화
        playerMove.animator.SetFloat("MoveX", 0f);
        playerMove.animator.SetFloat("MoveY", 0f);
        playerMove.animator.SetTrigger("Skill_Space");

        // 범위 표시 켜기
        ShowRangeIndicator();

        yield return new WaitForSeconds(0.4f);

        if (shinratenseiEffectPrefab != null)
        {
            Instantiate(shinratenseiEffectPrefab, transform.position, Quaternion.identity);
        }


        // 실제 판정: 원형 범위로 Obstacle 탐색
        Collider[] hits = Physics.OverlapSphere(transform.position, spaceRadius, detectLayer);

        foreach (Collider hit in hits)
        {
            Rigidbody obstacleRB = hit.attachedRigidbody;
            if (obstacleRB == null) continue;

            Vector3 dir = hit.transform.position - transform.position;
            dir.y = 10f;
            obstacleRB.AddForce(dir.normalized * 50f, ForceMode.Impulse);
            obstacleRB.gameObject.GetComponent<ObstacleCtrl_A>().DelayToDeActivate(2f); // 2초후 비활성화
        }

        yield return new WaitForSeconds(0.3f);
        // 범위 표시 끄기
        HideRangeIndicator();
        playerMove.isMoveLocked = false;

        // 쿨타임 시작
        StartCoroutine(AddForceCool_co());
    }

    // 스킬 범위 표시 메소드
    private void ShowRangeIndicator()
    {
        if (rangeIndicator == null) return;

        // 범위 오브젝트 위치 설정
        Vector3 pos = transform.position;
        pos.y = rangeIndicator.transform.position.y; // y는 미리 세팅된 높이 사용
        rangeIndicator.transform.position = pos;

        rangeIndicator.transform.localScale = new Vector3(spaceRadius * 2f, 1f, spaceRadius * 2f);

        rangeIndicator.SetActive(true);
    }

    private void HideRangeIndicator()
    {
        if (rangeIndicator == null) return;
        rangeIndicator.SetActive(false);
    }

    private IEnumerator AddForceCool_co()
    {
        yield return new WaitForSeconds(addForceCool);

        canSpaceSkill = true;
    }

    private IEnumerator IsTriggerCool_co()
    {
        yield return new WaitForSeconds(isTriggerCool);

        canShiftSkill = true;
    }
}