using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager_B : MonoBehaviour
{
    // 게임 진행 관련 변수들

    private float survivalTime = 0f; // 플레이어 생존 시간 (초 단위)
    private bool isGameOver = false; // 게임오버 상태인지 확인
    private GameOverUI gameOverUI; // 게임오버 UI 참조

    // 게임 시작 시 초기화 

    void Start()
    {
        // 씬에서 GameOverUI 컴포넌트 찾기
        gameOverUI = FindAnyObjectByType<GameOverUI>();
    }

    // 매 프레임마다 실행 

    void Update()
    {
        // 게임오버가 아닐 때만 시간 증가
        if (!isGameOver)
        {
            // Time.deltaTime: 이전 프레임과의 시간 간격 (초)
            // 매 프레임마다 deltaTime을 더해서 총 생존 시간 계산
            survivalTime += Time.deltaTime;
        }
        // Keyboard.current: 현재 연결된 키보드 // gKey: G키
        // wasPressedThisFrame: 이번 프레임에 눌렸는지
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            TriggerGameOver();
        }
    }

    // 게임오버 처리 함수 

    private void TriggerGameOver()
    {
        // 이미 게임오버 상태면 중복 실행 방지
        if (isGameOver)
        {
            return; // 함수 종료
        }

        // 게임오버 상태로 변경
        isGameOver = true;

        // GameOverUI에 생존 시간 전달하며 게임오버 화면 표시
        gameOverUI.ShowGameOver(survivalTime);
    }

    // 외부에서 호출 가능한 게임오버 함수 
    // 다른 스크립트에서 GameManager.GameOver()로 호출 가능

    public void GameOver()
    {
        TriggerGameOver(); // 내부 게임오버 함수 호출
    }
}

