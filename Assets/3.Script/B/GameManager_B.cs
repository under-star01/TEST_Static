using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager_B : MonoBehaviour
{
    // 인풋 액션 에셋 연결

    private float survivalTime = 0f; // 플레이어 생존시간 (�� ����)
    private bool isGameOver = false; // 게임 오버 상태
    private GameOverUI gameOverUI; // 게임오버 UI 참조
    private RankingViewUI rankingViewUI;

    // 플레이어 상태 관리 매니저 불러오기(죽음 상태 판단을 이쪽에서 하기 때문)
    [SerializeField] private PlayerState_A player;

    // 플레이어 상태 변환되면 구독자에게 알람보내줌
    private void Awake()
    {
        // 플레이어 자동으로 찾기
        player = FindAnyObjectByType<PlayerState_A>();

        if (player != null) // 플레이어가 없지 않으면
        {
            // 플레이어가 죽으면(구독자에게 액션-알람!) 게임오버 실행시켜!
            // 참고 - 하단에서 상황 종료시 구독 해제 해줌(상황 종료!)
            player.OnDie += GameOver;
        }

        DontDestroyOnLoad(gameObject); // 게임오브젝트야! 죽지마!~
    }

    // 게임 시작 시 초기화

    void Start()
    {
        // 씬에서 GameOverUI 컴포넌트 찾기
        gameOverUI = FindAnyObjectByType<GameOverUI>();
    }

    void Update()
    {
        // 게임오버가 아닐 때만 시간 증가
        if (!isGameOver)
        {
            // Time.deltaTime: 이전 프레임과의 시간 간격 (초)
            // 매 프레임마다 deltaTime을 더해서 총 생존 시간 계산
            survivalTime += Time.deltaTime;
        }

        //// 테스트용: G키 입력시 반응하게 - 뉴인풋이지만 올드처럼 스크립트에서 되도록
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    TriggerGameOver(); // 게임오버 메소드 호출
        //}

        // ESC키: 랭킹 보기 (추가)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (rankingViewUI != null)
            {
                rankingViewUI.OpenRankingView();
            }
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
        gameOverUI.ShowGameOver(survivalTime);
    }

    // 다른 스크립트에서 GameManager.GameOver()로 호출 가능

    public void GameOver()
    {
        TriggerGameOver(); // 내부 게임오버 메소드 호출
    }

    // 플레이어 사망 상황 종료(재시작 등)
    void OnDestroy()
    {
        if (player != null) // 플레이어가 없지 않으면?
        {
            player.OnDie -= GameOver; // 구독 해제
        }
    }


}

