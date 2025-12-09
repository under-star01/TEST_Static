using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // 인풋 액션 에셋 연결

    private bool isGameOver = false; // 게임 오버 상태
    private GameOverUI gameOverUI; // 게임오버 UI 참조
    private RankingViewUI rankingViewUI;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMPro.TextMeshProUGUI survivalTimeText;
    [SerializeField] private TMPro.TMP_InputField nameInputField;
    [SerializeField] private UnityEngine.UI.Button submitButton;
    [SerializeField] private UnityEngine.UI.Button restartButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;

    // Reset at GameStart
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Start()
    {
        Debug.Log($"[GameOver] Start 실행!");

        // GameManager가 준비될 때까지 대기하는 코루틴 시작
        StartCoroutine(WaitForGameManager());

    }

    private System.Collections.IEnumerator WaitForGameManager()
    {
        // GameManager.Instance가 null이 아닐 때까지 대기
        while (GameManager.Instance == null)
        {
            Debug.Log("[GameOver] GameManager 대기 중...");
            yield return null; // 한 프레임 대기
        }

        Debug.Log("[GameOver] GameManager 발견!");

        // GameOverUI 찾기
        gameOverUI = FindAnyObjectByType<GameOverUI>();
        Debug.Log($"[GameOver] GameOverUI 찾기 결과: {gameOverUI != null}");

        // 이벤트 구독
        Debug.Log("[GameOver] OnDie 이벤트 구독 시도!");
        GameManager.Instance.OnDie += ChkGameOver;
        Debug.Log("[GameOver] OnDie 이벤트 구독 완료!");
    }

    // 게임오버 처리 함수

    private void TriggerGameOver()
    {
        if (isGameOver) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[GameOver] TriggerGameOver 호출!");

        isGameOver = true;

        // GameManager의 생존시간 사용
        float finalTime = GameManager.Instance != null ?
            GameManager.Instance.survivalTime : 0f;

        Debug.Log($"[GameOver] 최종 시간: {finalTime}");

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver(finalTime);
        }
        else
        {
            Debug.LogError("[GameOver] GameOverUI가 null입니다!");
        }
    }

    // 다른 스크립트에서 GameManager.GameOver()로 호출 가능

    public void ChkGameOver()
    {
        Debug.Log("[GameOver] ChkGameOver 호출됨!");
        TriggerGameOver();
    }

    private void OnSubmitButtonClicked()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameOver] Submit 버튼 클릭됨");

    }

    private void OnRestartButtonClicked()
    {
        Debug.Log("[GameOver] Restart 버튼 클릭!");
        Time.timeScale = 1f;
        SceneManager.LoadScene("select");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("[GameOver] Quit 버튼 클릭!");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }





    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDie -= ChkGameOver;
        }
    }
}

