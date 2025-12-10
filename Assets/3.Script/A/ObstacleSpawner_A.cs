using System.Collections;
using System.Collections.Generic;
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
    public float delaySpawnTime = 0f; // 생성 딜레이 조절

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
        nextErrorSpawnTime = Random.Range(spawnErrorMin + delaySpawnTime, spawnErrorMax + delaySpawnTime);  
    }

    // Warning 타이머 설정 메소드
    private void ResetWarningTimer() // Warning: 1~5초
    {
        warningTimer = 0f;
        nextWarningSpawnTime = Random.Range(spawnWarningMin + delaySpawnTime, spawnWarningMax + delaySpawnTime); 
    }

    // Error 낙하물 활성화 메소드
    private void SpawnError()
    {
        int count = errorObject_List.Count;
        if (count == 0) return;

        int startIndex = Random.Range(0, count);
        GameObject selected = null;

        // 랜덤 인덱스 한 바퀴 검사
        for (int i = 0; i < count; i++)
        {
            int index = (startIndex + i) % count;
            GameObject obj = errorObject_List[index];

            if (!obj.activeSelf)
            {
                selected = obj;
                break;
            }
        }

        // 모든 오브젝트가 활성화 상태라면 → 스폰 스킵
        if (selected == null)
        {
            return;
        }

        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        float y = spawnHeight;

        selected.transform.position = new Vector3(x, y, z);
        selected.SetActive(true);
    }

    // Warning 낙하물 활성화 메소드
    private void SpawnWarning()
    {
        int count = warningObject_List.Count;
        if (count == 0) return;

        int startIndex = Random.Range(0, count);
        GameObject selected = null;

        // 랜덤 인덱스 한 바퀴 검사
        for (int i = 0; i < count; i++)
        {
            int index = (startIndex + i) % count;
            GameObject obj = warningObject_List[index];

            if (!obj.activeSelf)
            {
                selected = obj;
                break;
            }
        }

        // 모든 오브젝트가 활성화 상태라면 → 스폰 스킵
        if (selected == null)
        {
            return;
        }

        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        float y = spawnHeight;

        selected.transform.position = new Vector3(x, y, z);
        selected.SetActive(true);
    }
}
