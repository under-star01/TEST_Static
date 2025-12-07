using UnityEngine;

public class ObstacleCtrl_A : MonoBehaviour
{
    [SerializeField] private GameObject trace;
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float traceHeight = 2.4f;

    private bool initialized = false; // 맨 처음 생성할 때인지 여부

    private void OnEnable()
    {
        if (trace == null) return;

        if (!initialized)
        {
            // 처음 풀에 생성될 때는 부모 유지
            trace.SetActive(false); 
            initialized = true;
            return;
        }

        // y축 회전 랜덤 설정
        float randomY = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0f, randomY, 0f);

        // 시작시, 바닥에 범위 표시 설정
        trace.transform.SetParent(null);
        trace.SetActive(true);
        
        Vector3 trancePos = new Vector3(transform.position.x, -traceHeight, transform.position.z);
        trace.transform.position = trancePos;
    }

    private void OnDisable()
    {
        if (trace == null) return;

        trace.SetActive(false);
    }

    private void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 바닥과 부딪혔을 경우
        if (other.gameObject.CompareTag("Floor"))
        {
            // 비활성화 처리
            trace.transform.SetParent(transform);
            gameObject.SetActive(false);
        }
        // 플레이어와 부딪혔을 경우
        else if (other.gameObject.CompareTag("Player"))
        {
            // 밀어낼 방향 계산
            Vector3 dir = other.transform.position - transform.position;
            dir.y = 0f; // 수평으로만 플레이어를 밈
            dir.Normalize();

            // PlayerMove 스크립트의 ApplyKnockBack 함수 실행
            PlayerMove_A playerMove = other.GetComponent<PlayerMove_A>();
            
            if(playerMove != null)
            {
                Debug.Log("방해물과 충돌했어!");
                // 반대방향으로, 100f만큼 1초동안 넉백!
                playerMove.ApplyKnockBack(dir, 300f, 0.8f);
            }

            // 비활성화 처리
            trace.transform.SetParent(transform);
            gameObject.SetActive(false);
        }
    }
}
