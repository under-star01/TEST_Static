using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove_A : MonoBehaviour
{
    [Header("맵 범위")]
    [SerializeField] private Collider mapCollider; // 맵 오브젝트 콜라이더

    [Header("이동 관련 설정")]
    [SerializeField] private bool hasTarget = false; // 목적지 존재 여부
    [SerializeField] private float stopDistance = 0.1f; // 도착 판정 거리
    [SerializeField] private float moveSpeed = 5f; // 이동 속도

    private Rigidbody rb;
    private Animator animator;
    private Vector3 targetPos;
    private float minX, minZ, maxX,maxZ; // 맵 크기 변수

    private void Awake()
    {
        // 컴포넌트 연결
        TryGetComponent(out rb);
        animator = GetComponentInChildren<Animator>();

        // 맵 범위 적용
        if(mapCollider != null)
        {
            Bounds mapBound = mapCollider.bounds;
            
            minX = mapBound.min.x;
            maxX = mapBound.max.x;
            minZ = mapBound.min.z;
            maxZ = mapBound.max.z;
        }
        else
        {
            Debug.LogWarning("맵 범위 연결이 안됐어용");
        }
    }

    private void FixedUpdate()
    {
        // 목적지가 없다면 이동x
        if (!hasTarget)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // 타겟 거리에 따른 이동 제어
        Vector3 currentPos = transform.position;
        Vector3 toTarget = targetPos - currentPos;

         // 도착 거리 도달 o
        if(toTarget.magnitude < stopDistance)
        {
            hasTarget = false;
            rb.linearVelocity = Vector3.zero;
        }
         // 도착 거리 도달 x
        else
        {
            // 이동 및 회전 적용
            rb.linearVelocity = toTarget.normalized * moveSpeed;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion rot = Quaternion.LookRotation(toTarget, Vector3.up);
                rb.MoveRotation(rot);
            }
        }

        // 이동 시, 맵 범위로 제한 적용
        ClampPositionInsideMap();
    }

    private void Update()
    {
        // 속도에 따른 애니메이션 적용
        float speed = rb.linearVelocity.magnitude;

        animator.SetFloat("Speed", speed);
    }

    // 목적지 설정 메소드
    public void SetDestination(Vector3 target)
    {
        targetPos = target;
        hasTarget = true;
    }

    // 맵 크기로 이동 제한 메소드
    private void ClampPositionInsideMap()
    {
        if(mapCollider == null)
        {
            return;
        }

        Vector3 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        rb.position = pos;
    }
}
