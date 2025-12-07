using UnityEngine;

public class ProximityWall : MonoBehaviour
{
    [Header("투명도 설정")]
    [SerializeField] private float maxDistance = 10f;      // 완전 투명 거리
    [SerializeField] private float minDistance = 2f;       // 완전 불투명 거리
    [SerializeField] private float maxAlpha = 1f;          // 최대 투명도
    [SerializeField] private float minAlpha = 0f;          // 최소 투명도 
    [SerializeField] private float fadeSpeed = 5f;         // 투명도 변화 속도

    [Header("색상 설정")]
    [SerializeField] private Color wallColor = Color.white; // 벽 색상

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;

    private void Awake()
    {
        // SpriteRenderer 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 초기 색상 설정
        wallColor.a = minAlpha;
        spriteRenderer.color = wallColor;
        currentAlpha = minAlpha;
    }

    private void Start()
    {
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player 태그 없음");
        }
    }

    private void Update()
    {
        if (player == null || spriteRenderer == null)
            return;

        // 플레이어와의 거리 계산
        float distance = Vector3.Distance(transform.position, player.position);

        // 거리에 따른 목표 투명도 계산
        if (distance <= minDistance)
        {
            // 가까우면 완전 불투명
            targetAlpha = maxAlpha;
        }
        else if (distance >= maxDistance)
        {
            // 멀면 완전 투명
            targetAlpha = minAlpha;
        }
        else
        {
            // 중간 거리는 선형 보간
            float t = (distance - minDistance) / (maxDistance - minDistance);
            targetAlpha = Mathf.Lerp(maxAlpha, minAlpha, t);
        }

        // 부드럽게 투명도 변경
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // SpriteRenderer에 적용
        Color newColor = wallColor;
        newColor.a = currentAlpha;
        spriteRenderer.color = newColor;
    }

}