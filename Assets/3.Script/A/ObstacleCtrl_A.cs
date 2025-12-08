using System.Collections;
using UnityEngine;

public class ObstacleCtrl_A : MonoBehaviour
{
    [SerializeField] private GameObject trace;
    [SerializeField] private float fallSpeed = 10f;    // 초기 낙하 속도
    private float traceHeight = -2.4f;

    private bool initialized = false; // 맨 처음 생성할 때인지 여부
    private Rigidbody rb;
    private Coroutine destroyRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("ObstacleCtrl_A에 Rigidbody가 필요합니다!");
        }
    }

    private void OnEnable()
    {
        if (trace == null) return;

        if (!initialized)
        {
            // 처음 풀에 생성될 때는 부모 유지 + 숨기기
            trace.SetActive(false);
            initialized = true;
            return;
        }

        // y축 회전 랜덤 설정
        float randomY = Random.Range(0f, 360f);

        // 낙하 시작 속도 설정
        if (rb != null)
        {
            // 위에서 아래로 일정 속도로 떨어지게 (중력 + 초기 속도)
            rb.linearVelocity = Vector3.down * fallSpeed;
        }

        // 시작시, 바닥에 범위 표시 설정
        trace.transform.SetParent(null);

        Vector3 trancePos = new Vector3(transform.position.x, traceHeight, transform.position.z);
        trace.transform.position = trancePos;
        transform.rotation = Quaternion.Euler(0f, randomY, 0f);
        trace.transform.rotation = Quaternion.Euler(90f, randomY, 0f);

        trace.SetActive(true);
    }

    private void OnDisable()
    {
        if (trace != null)
        {
            trace.SetActive(false);
        }

        // 풀에 돌아갈 때 속도 초기화
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Floor 또는 DeadLine과 부딪혔을 경우
        if (other.gameObject.CompareTag("Floor") || other.gameObject.CompareTag("DeadLine"))
        {
            // 비활성화 처리
            gameObject.SetActive(false);
        }
        // 플레이어와 부딪혔을 경우
        else if (other.gameObject.CompareTag("Player"))
        {
            // 비활성화 처리
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Floor와 충돌시 범위 표시 비활성화
        if (collision.gameObject.CompareTag("Floor"))
        {
            if(destroyRoutine != null)
            {
                StopCoroutine(destroyRoutine);
            }

            // 20초 이후 자동 비활성화
            destroyRoutine = StartCoroutine(DeActiveToDelay(20f));
            // 범위 비활성화
            trace.SetActive(false);
        }
        else if(collision.gameObject.CompareTag("Player"))
        {
            // 비활성화 처리
            trace.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    // 지연 비활성화 메소드
    public void DelayToDeActivate(float duration)
    {
        StartCoroutine(DelayToDeActivate_co(duration));
    }

    // 지연 비활성화 코루틴
    private IEnumerator DelayToDeActivate_co(float duration)
    {
        // 일정 시간 대기
        yield return new WaitForSeconds(duration);
        
        // 비활성화
        gameObject.SetActive(false);
    }

    // 지연 비활성화 메소드
    private IEnumerator DeActiveToDelay(float duration)
    {
        // 일정 시간 대기
        yield return new WaitForSeconds(duration);

        gameObject.SetActive(false);
    }
}
