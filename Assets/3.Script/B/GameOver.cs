using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameOver : MonoBehaviour
{
    // 인풋 액션 에셋 연결

    private float survivalTime = 0f; // 플레이어 생존시간 (secends 단위)
    private bool isGameOver = false; // 게임 오버 상태
    private GameOverUI gameOverUI; // 게임오버 UI 참조
    private RankingViewUI rankingViewUI;

    // Reset at GameStart

    void Start()
    {
        Debug.Log($"[GameOver] Start 실행!");
        Debug.Log($"[GameOver] GameManager.Instance 존재 여부: {GameManager.Instance != null}");

        gameOverUI = FindAnyObjectByType<GameOverUI>();
        Debug.Log($"[GameOver] GameOverUI 찾기 결과: {gameOverUI != null}");

        if (GameManager.Instance != null)
        {
            Debug.Log("[GameOver] OnDie 이벤트 구독 시도!");
            GameManager.Instance.OnDie += ChkGameOver;
            Debug.Log("[GameOver] OnDie 이벤트 구독 완료!");
        }
        else
        {
            Debug.LogError("[GameOver] GameManager.Instance가 null입니다!");
        }
    }

    // 게임오버가 아닐 때만 시간 증가 > 게임매니저로 권한 이전


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

        // GameManager의 생존시간을 사용
        float finalTime = GameManager.Instance != null ?
            GameManager.Instance.survivalTime : 0f;

        // GameOverUI에 생존 시간 전달하며 게임오버 화면 표시
        gameOverUI.ShowGameOver(finalTime);
    }

    // 다른 스크립트에서 GameManager.GameOver()로 호출 가능

    public void ChkGameOver()
    {
        TriggerGameOver(); // 내부 게임오버 메소드 호출
    }

    private void OnDestroy()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnDie -= ChkGameOver;
        }
    }


}

