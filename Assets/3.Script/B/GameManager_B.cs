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

    // ���� ���� �� �ʱ�ȭ 

    void Start()
    {
        // 씬에서 GameOverUI 컴포넌트 찾기
        gameOverUI = FindAnyObjectByType<GameOverUI>();
    }

    // 게임오버가 아닐 때만 시간 증가

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

    // �ܺο��� ȣ�� ������ ���ӿ��� �Լ� 
    // 다른 스크립트에서 GameManager.GameOver()로 호출 가능

    public void GameOver()
    {
        TriggerGameOver(); // 내부 게임오버 메소드 호출
    }
}

