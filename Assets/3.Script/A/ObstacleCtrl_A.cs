using UnityEngine;

public class ObstacleCtrl_A : MonoBehaviour
{
    [SerializeField] private GameObject trace;
    [SerializeField] private float fallSpeed = 10f;

    private void Start()
    {
        // 시작시, 바닥에 경로 표시
        Vector3 trancePos = new Vector3(transform.position.x, 0.05f, transform.position.z);
        
        trace.transform.SetParent(null);
        trace.transform.position = trancePos;
        trace.gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Floor"))
        {
            trace.transform.SetParent(transform);
            Destroy(gameObject);
        }
    }
}
