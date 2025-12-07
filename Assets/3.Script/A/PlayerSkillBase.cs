using UnityEngine;

public abstract class PlayerSkillBase : MonoBehaviour
{
    protected PlayerState_A state;
    protected PlayerMove_A move;
    protected Rigidbody rb;
    protected Animator animator;

    protected virtual void Awake()
    {
        state = GetComponent<PlayerState_A>();
        move = GetComponent<PlayerMove_A>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (state == null)
        {
            Debug.LogWarning("PlayerSkillBase : PlayerState_A를 연결해주세용");
        }
        if (move == null)
        {
            Debug.LogWarning("PlayerSkillBase : PlayerMove_A를 연결해주세용");
        }
    }

    // Shift 버튼 스킬 메소드
    public abstract void UseSkill_Shift();

    // Space 버튼 스킬 메소드
    public abstract void UseSkill_Space();

    // 공통으로 쓸 수 있는 기능 정리 

    // 움직임 잠금 + 스킬 시전 상태 설정
    protected void BeginSkillCast(float moveLockDuration = -1f, float skillLockDuration = -1f)
    {
        if (state == null) return;

        state.SetCastingSkill(true);   // "스킬 시전 중" 플래그
        if (moveLockDuration > 0f)
            state.LockMove(moveLockDuration);   // 이동 잠금
        if (skillLockDuration > 0f)
            state.LockSkill(skillLockDuration); // 다른 스킬 잠금
    }

    // 스킬 시전 종료 처리
    protected void EndSkillCast()
    {
        if (state == null) return;

        state.SetCastingSkill(false);
        state.UnlockMove();
        state.UnlockSkill();
    }

    // 일정 시간 무적 부여
    protected void ActiveInvincible(float duration)
    {
        state.StartInvincible(duration);
    }

    // 애니메이션 트리거 공통 처리
    protected void PlaySkillAnimation(string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }
}