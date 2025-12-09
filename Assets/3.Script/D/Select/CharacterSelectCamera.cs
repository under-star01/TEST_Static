using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectCamera : MonoBehaviour
{
    [SerializeField]Transform[] camPoints;
    [SerializeField] Transform centerPoint;
    [SerializeField] Light[] spotlights;
    [SerializeField] CharacterData[] characterDataList; 

    public float rotationSpeed = 1f;
    private Camera mainCamera;
    private int currentIndex = 0;
    private bool isRotating = false;

    // 현재 선택된 캐릭터 정보를 가져오는 프로퍼티
    public CharacterData CurrentCharacter
    {
        get
        {
            if (characterDataList != null && currentIndex < characterDataList.Length)
                return characterDataList[currentIndex];
            return null;
        }
    }

    public int CurrentIndex => currentIndex;
    // UI 업데이트를 위한 이벤트
    public event Action<CharacterData> OnCharacterChanged;

    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.transform.position = camPoints[0].position;
        mainCamera.transform.rotation = camPoints[0].rotation;

        if (spotlights[0] != null && spotlights[1] != null)
        {
            spotlights[0].intensity = 40f;
            spotlights[1].intensity = 40f;
        }

        // 첫 캐릭터 정보 표시
        OnCharacterChanged?.Invoke(CurrentCharacter);
    }

    public void SelectLeft()
    {
        AudioManager.Instance.PlayButtonSFX();
        if (isRotating || camPoints.Length == 0) return;

        currentIndex = (currentIndex + 1) % camPoints.Length;
        StartCoroutine(MovingCamera(currentIndex));
    }

    public void SelectRight()
    {
        AudioManager.Instance.PlayButtonSFX();
        if (isRotating || camPoints.Length == 0) return;

        currentIndex--;
        if (currentIndex < 0) currentIndex = camPoints.Length - 1;
        StartCoroutine(MovingCamera(currentIndex));
    }

    public IEnumerator MovingCamera(int targetindex)
    {
        isRotating = true;
        if (spotlights[0] != null && spotlights[1] != null)
        {
            spotlights[0].intensity = 0f;
            spotlights[1].intensity = 0f;
        }

        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        Vector3 targetPosition = camPoints[targetindex].position;
        Quaternion targetRotation = camPoints[targetindex].rotation;

        Vector3 center = centerPoint != null ? centerPoint.position : Vector3.zero;

        float fixedY = startPosition.y;

        float elapsed = 0f;
        float duration = 1f / rotationSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            // XZ 평면에서만 원형 이동
            Vector3 startDir = new Vector3(startPosition.x - center.x, 0, startPosition.z - center.z);
            Vector3 targetDir = new Vector3(targetPosition.x - center.x, 0, targetPosition.z - center.z);
            Vector3 currentDir = Vector3.Slerp(startDir, targetDir, t);

            // Y축 완전 고정
            mainCamera.transform.position = new Vector3(
                center.x + currentDir.x,
                fixedY,
                center.z + currentDir.z
            );

            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        isRotating = false;
        if (spotlights[0] != null && spotlights[1] != null)
        {
            spotlights[0].intensity = 40f;
            spotlights[1].intensity = 40f;
        }

        // 캐릭터 변경 이벤트 발생
        OnCharacterChanged?.Invoke(CurrentCharacter);
    }
}
