using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObstacleSpawner_A : MonoBehaviour
{
    [Header("맵 범위")]
    [SerializeField] private Collider mapCollider;

    [Header("낙하물 설정")]
    [SerializeField] private List<GameObject> errorPrefab_List; // 에러 오브젝트 프리팹 리스트
    [SerializeField] private List<GameObject> warningPrefab_List; // 경고 오브젝트 프리팹 리스트
    [SerializeField] private List<GameObject> errorObject_List; // 에러 오브젝트 리스트
    [SerializeField] private List<GameObject> warningObject_List; // 경고 오브젝트 리스트
    
    [SerializeField] private int spawnErrorCnt = 10;   // 에러 오브젝트 생성 개수
    [SerializeField] private int spawnWarningCnt = 10;   // 경고 오브젝트 생성 개수
    [SerializeField] private float spawnHeight = 10f; // 맵 위 얼마나 위에서 생성할지

    [SerializeField] private float spawnErrorMin = 3f; // 에러 오브젝트 최소 생성 시간
    [SerializeField] private float spawnErrorMax = 5f; // 에러 오브젝트 최대 생성 시간
    [SerializeField] private float spawnWarningMin = 1f; // 경고 오브젝트 최소 생성 시간
    [SerializeField] private float spawnWarningMax = 5f; // 경고 오브젝트 최대 생성 시간

    // 타이머
    private float errorTimer;
    private float warningTimer;

    // 다음 스폰 시간
    private float nextErrorSpawnTime;
    private float nextWarningSpawnTime;

    // 맵 범위 제한 변수
    private float minX, maxX, minZ, maxZ, topY;

    private void Start()
    {
        // 맵 정보 확인
        if (mapCollider == null)
        {
            Debug.LogWarning("맵 범위가 Spawner에 연결되지 않았어용");
            enabled = false;
            return;
        }

        // 생성 범위 제한 설정
        Bounds b = mapCollider.bounds;
        minX = b.min.x;
        maxX = b.max.x;
        minZ = b.min.z;
        maxZ = b.max.z;

        // 오브젝트 각각 10개씩 생성 및 리스트에 추가
        for(int i=0; i < errorPrefab_List.Count; i++)
        {
            for (int j = 0; j < spawnErrorCnt; j++)
            {
                GameObject error = Instantiate(errorPrefab_List[i], transform.position, Quaternion.identity);
                errorObject_List.Add(error);
                error.transform.SetParent(transform);
                error.SetActive(false);
            }
        }
        for (int i = 0; i < warningPrefab_List.Count; i++)
        {
            for (int j = 0; j < spawnWarningCnt; j++)
            {
                GameObject warning = Instantiate(warningPrefab_List[i], transform.position, Quaternion.identity);
                warningObject_List.Add(warning);
                warning.transform.SetParent(transform);
                warning.SetActive(false);
            }
        }


    }

    private void Update()
    {
        errorTimer += Time.deltaTime;
        warningTimer += Time.deltaTime;

        // Error 낙하물 활성화
        if (errorTimer >= nextErrorSpawnTime)
        {
            SpawnError();
            ResetErrorTimer();
        }

        // Warning 낙하물 활성화
        if (warningTimer >= nextWarningSpawnTime)
        {
            SpawnWarning();
            ResetWarningTimer();
        }
    }

    // Error 타이머 설정 메소드
    private void ResetErrorTimer() // Error: 3~5초
    {
        errorTimer = 0f;
        nextErrorSpawnTime = Random.Range(spawnErrorMin, spawnErrorMax);  
    }

    // Warning 타이머 설정 메소드
    private void ResetWarningTimer() // Warning: 1~5초
    {
        warningTimer = 0f;
        nextWarningSpawnTime = Random.Range(spawnWarningMin, spawnWarningMax); 
    }

    // Error 낙하물 활성화 메소드
    private void SpawnError()
    {
        // 생성 위치의 XZ는 랜덤
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);

        // Y는 spawnHeight만큼 위로 설정
        float y = spawnHeight;

        foreach (GameObject o in errorObject_List)
        {
            // 비활성화된 오브젝트가 있을 경우, 활성화
            if (!o.activeSelf)
            {
                Vector3 pos = new Vector3(x, y, z);
                o.transform.position = pos;
                o.SetActive(true);
                break;
            }
        }
    }

    // Warning 낙하물 활성화 메소드
    private void SpawnWarning()
    {
        // 생성 위치의 XZ는 랜덤
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);

        // Y는 spawnHeight만큼 위로 설정
        float y = spawnHeight;

        foreach (GameObject o in warningObject_List)
        {
            // 비활성화된 오브젝트가 있을 경우, 활성화
            if (!o.activeSelf)
            {
                Vector3 pos = new Vector3(x, y, z);
                o.transform.position = pos;
                o.SetActive(true);
                break;
            }
        }
    }
}
