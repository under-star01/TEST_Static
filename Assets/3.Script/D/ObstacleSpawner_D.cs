using UnityEngine;

public class ObstacleSpawner_D : MonoBehaviour
{
    [Header("맵 범위")]
    [SerializeField] private Collider mapCollider;

    [Header("낙하물 설정")]
    [SerializeField] private GameObject dropPrefab;   // 떨어질 오브젝트 프리팹
    [SerializeField] private float spawnHeight = 10f; // 맵 위 얼마나 위에서 생성할지
    [SerializeField] private float spawnInterval = 1.0f;

    private float timer;

    private float minX, maxX, minZ, maxZ, topY;

    private void Start()
    {
        if (mapCollider == null)
        {
            Debug.LogWarning("맵 범위가 Spawner에 연결되지 않았어용");
            enabled = false;
            return;
        }

        Bounds b = mapCollider.bounds;

        minX = b.min.x;
        maxX = b.max.x;
        minZ = b.min.z;
        maxZ = b.max.z;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnDrop();
        }
    }

    private void SpawnDrop()
    {
        // 생성 위치의 XZ는 랜덤
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);

        // Y는 spawnHeight만큼 위로 설정
        float y = spawnHeight;

        Vector3 pos = new Vector3(x, y, z);
        Instantiate(dropPrefab, pos, Quaternion.Euler(-90f, Random.Range(0f, 360f), 0f));
    }
}
