using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using System;
using Game.UI;
using TMPro;
using I18N.Common;
using UnityEngine.Playables;
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
    [SerializeField] private float memorySpeed = 1f; // 메모리 차는 속도

    public float memoryGauge = 0f; // 0 ~ 100

    [Header("누적 발생한 오류의 수")]
    [SerializeField] private int maxHpCnt = 3; // 누적 발생한 오류의 수
    [SerializeField] private int hpCnt = 3; // 누적 발생한 오류의 수

    [Header("생존 시간 설정")]
    public float survivalTime = 0f; // 플레이어 생존시간 (secends 단위)
    public bool isGameOver = false; // 게임 오버 상태

    [SerializeField] private List<GameObject> playerPrefabs_List; // 플레이어 프리팹 리스트
    [SerializeField] private TextMeshProUGUI survivalTimeUI;

    private PlayerInputActions inputActions;
    private GameOverUI gameOverUI; // 게임오버 UI 참조
    private RankingViewUI rankingViewUI;
    public event Action OnDie;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 메인 씬에서 바로 플레이어 생성
        SpawnPlayerFromSavedData();
    }

    void Start()
    {
        // 씬에서 GameOverUI 컴포넌트 찾기
        gameOverUI = FindAnyObjectByType<GameOverUI>();
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
    }

    private void UpdateGauge()
    {
        if (isGameOver) return;

        // 생존 시간 갱신
        survivalTime += Time.deltaTime;
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
            OnDie?.Invoke();  // 사망 이벤트 호출
        }

        //UIManager.Instance.UpdateMemoryUI(memoryGauge);
    }

    // 피격 반응 메소드
    public void TakeDamage(int cnt)
    {
        if (hpCnt <= 0)
        {
            Debug.Log("Hp가 소모되어 메모리 사용량 증가폭이 늘어나지 않습니다.");
        }
        else
        {
            hpCnt -= cnt;
            Debug.Log($"현재 체력 [{hpCnt}] : 플레이어의 HP가 감소합니다!");
        }
    }

    // 게임오버 처리 함수
    private void TriggerGameOver()
    {
        // 이미 게임오버 상태면 중복 실행 방지
        if (isGameOver)
        {
            return; // 메서드 종료
        }

        // 게임오버 상태 켜기
        isGameOver = true;

        // GameOverUI에 생존 시간 전달하며 게임오버 화면 표시
        //gameOverUI.ShowGameOver(survivalTime);
    }

    // 다른 스크립트에서 GameManager.GameOver()로 호출 가능

    private void GameOver()
    {
        // 게임오버 상태 켜기
        isGameOver = true;
        //UIManager.Instance.ShowGameOverUI();
    }
}
