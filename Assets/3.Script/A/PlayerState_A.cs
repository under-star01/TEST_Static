using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_A : MonoBehaviour
{
    [Header("HP 설정")]
    [SerializeField] private int maxHP = 3;
    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;
    public bool IsDead => CurrentHP <= 0;

    public event Action<int, int> OnHPChanged;
    public event Action OnDie; 

    [Header("상태 플래그")]
    public bool IsInvincible { get; private set; }
    public bool IsKnockback { get; private set; }
    public bool IsCastingSkill { get; private set; }

    private bool moveLocked;
    private bool skillLocked;

    public bool CanMove => !IsDead && !IsKnockback && !moveLocked;
    public bool CanUseSkill => !IsDead && !IsCastingSkill && !skillLocked;

    [Header("기본 지속시간")]
    [SerializeField] private float defaultInvincibleTime = 0.3f;

    // 코루틴 저장
    private Coroutine invincibleRoutine;
    private Coroutine knockbackRoutine;
    private Coroutine moveLockRoutine;
    private Coroutine skillLockRoutine;

    // 이벤트 종류
    public event Action OnInvincibleStart; // 무적 시작 이벤트
    public event Action OnInvincibleEnd; // 무적 종료 이벤트
    public event Action OnKnockbackStart; // 넉백 시작 이벤트
    public event Action OnKnockbackEnd; // 넉백 종료 이벤트

    private void Awake()
    {
        // HP 초기화
        CurrentHP = maxHP;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    // HP 처리
    public void TakeDamage(int amount)
    {
        if (IsDead || IsInvincible) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        OnHPChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0)
            Die();
    }

    private void Die()
    {
        OnDie?.Invoke();
        moveLocked = true;
        skillLocked = true;
    }

    // 무적 적용
    public void StartInvincible(float duration = -1f)
    {
        if (duration <= 0f)
            duration = defaultInvincibleTime;

        if (invincibleRoutine != null)
            StopCoroutine(invincibleRoutine);

        invincibleRoutine = StartCoroutine(InvincibleRoutine(duration));
    }

    private IEnumerator InvincibleRoutine(float duration)
    {
        IsInvincible = true;
        OnInvincibleStart?.Invoke();

        yield return new WaitForSeconds(duration);

        IsInvincible = false;
        OnInvincibleEnd?.Invoke();

        invincibleRoutine = null;
    }

    // 넉백 적용
    public void StartKnockback(float duration)
    {
        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(duration));
    }

    private IEnumerator KnockbackRoutine(float duration)
    {
        IsKnockback = true;
        OnKnockbackStart?.Invoke();

        yield return new WaitForSeconds(duration);

        IsKnockback = false;
        OnKnockbackEnd?.Invoke();

        knockbackRoutine = null;
    }

    public void StopKnockback()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        IsKnockback = false;
        OnKnockbackEnd?.Invoke();
    }

    // 이동 잠금
    public void LockMove(float duration)
    {
        if (moveLockRoutine != null)
            StopCoroutine(moveLockRoutine);

        moveLockRoutine = StartCoroutine(MoveLockRoutine(duration));
    }

    private IEnumerator MoveLockRoutine(float duration)
    {
        moveLocked = true;
        yield return new WaitForSeconds(duration);
        moveLocked = false;
        moveLockRoutine = null;
    }

    public void UnlockMove()
    {
        if (moveLockRoutine != null)
        {
            StopCoroutine(moveLockRoutine);
            moveLockRoutine = null;
        }
        moveLocked = false;
    }

    // 스킬 잠금
    public void LockSkill(float duration)
    {
        if (skillLockRoutine != null)
            StopCoroutine(skillLockRoutine);

        skillLockRoutine = StartCoroutine(SkillLockRoutine(duration));
    }

    private IEnumerator SkillLockRoutine(float duration)
    {
        skillLocked = true;
        yield return new WaitForSeconds(duration);
        skillLocked = false;
        skillLockRoutine = null;
    }

    public void UnlockSkill()
    {
        if (skillLockRoutine != null)
        {
            StopCoroutine(skillLockRoutine);
            skillLockRoutine = null;
        }
        skillLocked = false;
    }

    // 스킬 시전 상태
    public void SetCastingSkill(bool isCasting)
    {
        IsCastingSkill = isCasting;
    }
}
