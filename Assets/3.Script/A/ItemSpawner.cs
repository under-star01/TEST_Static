using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ItemSpawnData
    {
        public string itemName;                    
        public GameObject itemPrefab; // 생성할 아이템 프리팹
        public List<Transform> spawnPoints; // 이 아이템이 생성될 위치 리스트
        public float minSpawnInterval = 3f; // 최소 생성 간격
        public float maxSpawnInterval = 5f; // 최대 생성 간격
    }

    [Header("GC 아이템 설정")]
    [SerializeField] private ItemSpawnData gcItem;

    [Header("URP 아이템 설정")]
    [SerializeField] private ItemSpawnData urpItem;

    [Header("Caching 아이템 설정")]
    [SerializeField] private ItemSpawnData cachingItem;

    private void Start()
    {
        // 아이템별 각각 독립적인 스폰 코루틴 시작
        StartCoroutine(SpawnRoutine(gcItem));
        StartCoroutine(SpawnRoutine(urpItem));
        StartCoroutine(SpawnRoutine(cachingItem));
    }

    private IEnumerator SpawnRoutine(ItemSpawnData data)
    {
        while (true)
        {
            // 대기 시간 랜덤 결정
            float waitTime = Random.Range(data.minSpawnInterval, data.maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            if (data.itemPrefab == null)
            {
                Debug.LogWarning($"{data.itemName} : itemPrefab이 비어있습니다.");
                continue;
            }

            if (data.spawnPoints == null || data.spawnPoints.Count == 0)
            {
                Debug.LogWarning($"{data.itemName} : spawnPoints 리스트가 비어있습니다.");
                continue;
            }

            // 랜덤 위치 선택
            int index = Random.Range(0, data.spawnPoints.Count);
            Transform spawnPoint = data.spawnPoints[index];

            // Instantiate
            Instantiate(data.itemPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}