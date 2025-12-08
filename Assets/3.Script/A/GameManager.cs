using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using System;
using Game.UI;
using TMPro;
using I18N.Common;
using UnityEngine.Playables;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    [Header("메모리 게이지 설정")]
    public float memoryGauge = 0f; // 0 ~ 100
    [SerializeField] private float baseIncreaseRate = 1f; // hpCnt 3칸일 때 기본 속도
    private float currentIncreaseRate; // 현재 증가 속도

    [Header("누적 발생한 오류의 수")]
    [SerializeField] private int hpCnt = 3; // 누적 발생한 오류의 수
    public int HpCnt
    {
        get => hpCnt;
        set
        {
            hpCnt = value;
            // 변경된 Hp에 따른 증가 속도 적용
            HandleHPChanged(hpCnt, 3);
        }
    }

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

        // InputAction 생성
        inputActions = new PlayerInputActions();

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
        
        // HP에 비례한 속도 증가
        memoryGauge += currentIncreaseRate * Time.deltaTime;
        memoryGauge = Mathf.Clamp(memoryGauge, 0f, 100f);

        //UIManager.Instance.UpdateMemoryUI(memoryGauge);
    }

    private void HandleHPChanged(int currentHP, int maxHP)
    {
        // HP에 따라 속도 조절
        currentIncreaseRate = baseIncreaseRate * (maxHP - currentHP + 1);
        // 예: HP=3 → 1배, HP=2 → 2배, HP=1 → 3배 …
        Debug.Log($"메모리 증가속도 변경: {currentIncreaseRate}");
    }


    // 피격 반응 메소드
    public void TakeDamage(int cnt)
    {
        if (hpCnt <= 0)
        {
            OnDie?.Invoke();  // 사망 이벤트 호출
        }
        else
        {
            HpCnt -= cnt;
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
