using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill_RigidBody : PlayerSkillBase
{
    [Header("Space 스킬 설정")]
    [SerializeField] private float spaceRadius = 5f; // 스킬 범위
    [SerializeField] private LayerMask detectLayer; // 탐지 레이어

    [Header("범위 표시 오브젝트")]
    [SerializeField] private GameObject rangeIndicator; // 붉은 원 Mesh/Quad (미리 프리팹 or 자식으로 세팅)

    private bool isShiftRunning = false;

    // Shift: 일정 시간 완전 무적 스킬
    public override void UseSkill_Shift()
    {
        if (!state.CanUseSkill) return;

        Debug.Log("Shift 스킬 사용!");
        ActiveInvincible(5.0f); // 5초 무적
        PlaySkillAnimation("Skill_Shift");
    }

    // Space 스킬 실행 메소드 (주변 방해물 날려버리기)
    public override void UseSkill_Space()
    {
        if (!state.CanUseSkill) return;
        if (isShiftRunning) return; // 연속 입력 방지

        StartCoroutine(UseSpaceRoutine());
    }

    // Space 스킬 실행 코루틴 (주변 방해물 날려버리기)
    private IEnumerator UseSpaceRoutine()
    {
        isShiftRunning = true;

        Debug.Log("Space 스킬 사용!");
        BeginSkillCast(moveLockDuration: 0.5f); // 0.5초 동안 제자리
        ActiveInvincible(0.5f);                 // 0.5초 동안 무적
        PlaySkillAnimation("Skill_Space");

        // 범위 표시 켜기
        ShowRangeIndicator();
        yield return new WaitForSeconds(0.2f);

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

        // 범위 표시 끄기
        HideRangeIndicator();

        // 캐스팅 종료
        EndSkillCast();

        isShiftRunning = false;
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
}
