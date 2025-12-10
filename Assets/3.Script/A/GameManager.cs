using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Game.UI;
using TMPro;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    [Header("메모리 게이지 설정")]
    [SerializeField] private Slider memorySlider; // 메모리 사용량 Slider
    [SerializeField] private float baseIncreaseRate = 1f; // hpCnt 3칸일 때 기본 속도
    [SerializeField] private float currentIncreaseRate
    {
        get
        {
            return baseIncreaseRate * (maxHpCnt - hpCnt + 1);
        }
    }
    [SerializeField] private float debugCurrentIncreaseRate;
    [SerializeField] private float memorySpeed = 1.5f; // 메모리 차는 속도

    public float memoryGauge = 0f; // 0 ~ 100

    [Header("누적 발생한 오류의 수")]
    [SerializeField] private int maxHpCnt = 3; // 누적 발생한 오류의 수
    [SerializeField] private int hpCnt = 3; // 누적 발생한 오류의 수
    [SerializeField] private List<GameObject> hpUI_List; // HP 체력 UI 리스트

    [Header("생존 시간 설정")]
    public float survivalTime = 0f; // 플레이어 생존시간 (secends 단위)
    public bool isGameOver = false; // 게임 오버 상태

    [Header("초기화 데이터")]
    [SerializeField] private List<GameObject> playerPrefabs_List; // 플레이어 프리팹 리스트
    [SerializeField] private List<GameObject> skillUI_List; // 스킬UI 리스트
    [SerializeField] private List<Image> skillUIImage; // 스킬UI 이미지

    [SerializeField] private ObstacleSpawner_A obstacleSpawner; // 낙하물 생성 스크립트
    [SerializeField] private TextMeshProUGUI survivalTimeUI;

    private GameOverUI gameOverUI; // 게임오버 UI 참조
    private RankingViewUI rankingViewUI;
    
    public event Action OnDie; // 게임오버 이벤트(구독 가능)

    private Coroutine UseURP;
    private Coroutine UseCaching;


    private void Awake()
    {
        Debug.Log("[GameManager] Awake 실행!");
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] 중복 인스턴스 파괴!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[GameManager] Instance 설정 완료!");


        // 메인 씬에서 바로 플레이어 생성
        SpawnPlayerFromSavedData();
    }

    private void Update()
    {
        UpdateGauge();
    }

    private void SpawnPlayerFromSavedData()
    {
        // PlayerPrefs에서 선택값 읽기
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);

        // 스폰 위치 결정
        Vector3 spawnPos = Vector3.zero;

        // 실제 플레이어 생성
        Instantiate(playerPrefabs_List[selectedIndex], spawnPos, Quaternion.identity);

        // 스킬 UI 연결
        if(selectedIndex == 0)
        {
            // Transform 스킬 연결
            skillUI_List[0].SetActive(true);
            skillUI_List[1].SetActive(true);
        }
        else if (selectedIndex == 1)
        {
            // RigidBody 스킬 연결
            skillUI_List[2].SetActive(true);
            skillUI_List[3].SetActive(true);
        }
        else if (selectedIndex == 2)
        {
            // Coroutine 스킬 연결
            skillUI_List[4].SetActive(true);
            skillUI_List[5].SetActive(true);
        }
    }

    private void UpdateGauge()
    {
        if (isGameOver) return;

        // 생존 시간 갱신
        survivalTime += Time.deltaTime;

        // UI 업데이트
        int minutes = (int)(survivalTime / 60);
        int seconds = (int)(survivalTime % 60);
        int milli = (int)((survivalTime % 1) * 100);
        survivalTimeUI.text = $"{minutes:00} : {seconds:00} : {milli:00}";
        

        // HP에 비례한 메모리 증가 속도↑
        memoryGauge += currentIncreaseRate * memorySpeed * Time.deltaTime;
        memoryGauge = Mathf.Clamp(memoryGauge, 0f, 100f);
        memorySlider.value = memoryGauge;
        debugCurrentIncreaseRate = currentIncreaseRate;

        if (memoryGauge >= 100 && !isGameOver)
        {
            isGameOver = true;
            Debug.LogWarning("[GameOver] : 게임 오버 이벤트 실행!!");

            OnDie?.Invoke();  // 이벤트도 호출 (나중을 위해)
        }
    }

    // 피격 반응 메소드
    public void TakeDamage(int cnt)
    {
        hpCnt -= cnt;
        hpCnt = Mathf.Clamp(hpCnt, 0, 3);

        if (hpCnt <= 0)
        {
            // HP UI갱신
            for (int i = 0; i < hpUI_List.Count; i++)
            {
                hpUI_List[i].SetActive(false);
            }
            Debug.Log("Hp가 소모되어 메모리 사용량 증가폭이 늘어나지 않습니다.");
        }
        else
        {
            Debug.Log(hpCnt);

            // HP UI갱신
            for(int i=0; i< hpUI_List.Count - hpCnt; i++)
            {
                hpUI_List[i].SetActive(false);
            }
            Debug.Log($"현재 체력 [{hpCnt}] : 플레이어의 HP가 감소합니다!");
        }
    }

    // 아이템 사용 : GC -> hpCnt 회복
    public void UseItem_GC()
    {
        if (isGameOver) return;

        if(hpCnt >= 3)
        {
            Debug.Log("현재 최대 HP이므로 더이상 증가할 수 없습니다!");
            memoryGauge -= 5f;
        }
        else
        {
            hpCnt++;
            memoryGauge -= 5f;

            // HP UI갱신
            for (int i = hpUI_List.Count - 1; i >= hpCnt; i--)
            {
                hpUI_List[i].SetActive(false);
            }

            for (int i = hpUI_List.Count - 1; i >= hpUI_List.Count - hpCnt; i--)
            {
                hpUI_List[i].SetActive(true);
            }
        }
    }

    // 아이템 사용 : URP -> 떨어지는 시간 증가
    public void UseItem_URP()
    {
        if (UseURP != null)
        {
            StopCoroutine(UseURP);
        }

        if (obstacleSpawner != null)
        {
            memoryGauge -= 2f;
            // 7초동안 지속
            UseURP = StartCoroutine(UseItem_URP_co(7f));
        }
    }

    private IEnumerator UseItem_URP_co(float delay)
    {
        Physics.gravity = new Vector3(0f, -3f, 0f);
        yield return new WaitForSeconds(delay);

        Physics.gravity = new Vector3(0f, -5f, 0f);
    }


    // 아이템 사용 : Caching -> 생성되는 오류 간격 증가
    public void UseItem_Caching()
    {
        if(UseCaching != null)
        {
            StopCoroutine(UseCaching);
        }

        if (obstacleSpawner != null)
        {
            memoryGauge -= 2f;
            // 7초동안 지속
            UseCaching = StartCoroutine(UseItem_Caching_co(7f));
        }
    }

    private IEnumerator UseItem_Caching_co(float delay)
    {
        obstacleSpawner.delaySpawnTime = 1f;
        yield return new WaitForSeconds(delay);

        obstacleSpawner.delaySpawnTime = 0f;
    }

    public void SkillUIUpdate(int skillUIIndex, float delay)
    {
        StartCoroutine(SkillUIUpdate_co(skillUIIndex, delay));
    }

    private IEnumerator SkillUIUpdate_co(int skillUIIndex, float delay)
    {
        Slider skillUI = skillUI_List[skillUIIndex].GetComponent<Slider>();
        
        skillUI.value = 0f;
        float elapsed = 0f;

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / delay);
            skillUI.value = t * skillUI.maxValue; ;
            yield return null;
        }

        // 정확히 끝 값으로 보정
        skillUI.value = skillUI.maxValue;
    }

    public void ChangeSkillUIColor(int skiilUIIndex, bool isYeloow)
    {
        if (isYeloow)
        {
            skillUIImage[skiilUIIndex].color = Color.yellow;
        }
        else
        {
            skillUIImage[skiilUIIndex].color = Color.white;
        }
    }

    

}
