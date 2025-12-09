using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Ranking;

namespace Game.UI
{
    public class GameOverUI : MonoBehaviour
    {
        // UI 요소들을 인스펙터 창에서 연결하기 위한 변수들

        [Header("Main Panels")]
        [SerializeField] private GameObject gameOverPanel; // 게임오버 전체 패널
        [SerializeField] private GameObject nameInputPanel; // 이름 인풋 패널

        [Header("Player Record")]
        [SerializeField] private TextMeshProUGUI recordTimeText; // 기록(생존 시간) 텍스트
        [SerializeField] private TextMeshProUGUI recordMessageText; // New record 띄울 때 쓸것

        [Header("Name Input")]
        [SerializeField] private TMP_InputField nameInputField; // 인풋필드 - TMP로 만든 오브젝트에 입력받을 필드
        [SerializeField] private Button submitButton; // 제출 버튼

        [Header("Ranking")]
        [SerializeField] private Transform rankingContent; // 랭킹요소에 들어갈 부모오브젝트
        [SerializeField] private GameObject rankItemPrefab; // 랭킹을 만들 프리팹

        [Header("Buttons")]
        [SerializeField] private Button restartButton; // 재시작버튼
        [SerializeField] private Button quitButton; //종료버튼

        [Header("Settings")]
        [SerializeField] private int topRankFontSize = 32; // 상위3등 폰트크기
        [SerializeField] private int normalRankFontSize = 24; // 나머지 7개 출력크기
        [SerializeField] private Color topRankColor = Color.yellow; // 3등 색상 - 노랑
        [SerializeField] private Color normalRankColor = Color.white; // 나머지 - 흰색

        private RankingManager rankingManager; // 랭킹매니저
        private float playerScore; // 플레이어 기록
        private bool isNewRecord = false; // 신기록인지 체크

        void Start()
        {
            // 랭킹 데이터 찾아서 변수 저장
            rankingManager = FindAnyObjectByType<RankingManager>();

            submitButton.onClick.AddListener(OnSubmitName); // 이름 제출 버튼 누르면 ()실행
            restartButton.onClick.AddListener(OnRestart); // 재시작 버튼 누르면 ()실행
            quitButton.onClick.AddListener(OnQuit); // 종료 버튼 누르면 ()실행

            //시작할때 게임오버 패널 관련 종료시켜놓기
            gameOverPanel.SetActive(false); // 게임오버 패널 끄기
            nameInputPanel.SetActive(false); // 이름 입력 패널 끄기
        }

        public void ShowGameOver(float score) // 게임오버시 호출할 메서드
        {
            playerScore = score; // 전달받은 점수 저장
            gameOverPanel.SetActive(true); // 게임오버 패널 켜기

            recordTimeText.text = FormatTime(score); // 생존 시간 저장( 분 : 초 : 밀리초 저장 - 변경 및 추가 가능)

            isNewRecord = CheckIfNewRecord(score); // 신기록(10위안)인지 체크

            if (isNewRecord)
            {
                // 신기록일 경우 아래 띄움
                recordMessageText.text = "NEW RECORD!";
                recordMessageText.color = Color.green;
                DisplayRanking(); // 랭킹 먼저 표시
                ShowNameInput(); // 그리고 이름 입력창 띄우기
            }
            else
            { 
                // 순위에 못들었으면
                recordMessageText.text = "Try Again!";
                recordMessageText.color = Color.gray;
                DisplayRanking(); // 랭킹띄움
            }

            Time.timeScale = 0f; // 게임 일시정지(게임오버 창 떠있는 동안)
        }

        private string FormatTime(float timeInSeconds) // 시간 저장받을 메서드
        {
            int minutes = (int)(timeInSeconds / 60f); // 분 계산
            int seconds = (int)(timeInSeconds % 60f); // 초 계산
            int milliseconds = (int)((timeInSeconds * 100f) % 100f); // 밀리초 계산

            return minutes.ToString("00") + ":" + seconds.ToString("00") + "." + milliseconds.ToString("00");
            // 반환 - 분 : 초 : 밀리초
        }

        private bool CheckIfNewRecord(float score) // 신기록인지 체크하는 메서드
        {
            // 랭킹 매니저에서 랭킹 데이터 받아오기
            RankingData data = rankingManager.GetRankingData();

            // 기록된 값이 10개 미만이면 무조건 넣기
            if (data.entries.Count < 10)
            {
                return true;
            }

            // 마지막(10위) 인덱스 계산 > 리스트는 0부터 시작이니까 1빼야함.
            int lastIndex = data.entries.Count - 1;
            // 10위보다 기록 높으면 신기록!
            return score > data.entries[lastIndex].score;
        }

        private void ShowNameInput() // 이름 입력 패널 띄우기
        {
            nameInputPanel.SetActive(true); // 이름 입력 패널 초기화
            nameInputField.text = ""; // 입력 필드 비우기 (이전 텍스트 제거)
        }

        private void OnSubmitName() // 제출 버튼 클릭시 메서드
        {
            // 입력 필드에서 이름 받아오기
            string playerName = nameInputField.text;

            if (playerName == "")
            {
                // 이름 빈칸이면 게스트로 저장
                playerName = "GUEST";
            }

            if (playerName.Length > 5)
            {
                // 5글자까지만 입력받기
                playerName = playerName.Substring(0, 5);
            }
           
            rankingManager.SubmitScore(playerName, playerScore); // 랭킹 매니저에 이름, 점수 보내서 저장
            nameInputPanel.SetActive(false); // 입력 패널 끄기
            DisplayRanking(); // 랭킹창 띄움
        }

        private void DisplayRanking() // 랭킹창 띄우기
        {
            // 이전에 띄웠던 남아있는 랭킹 관련 삭제(초기화)
            foreach (Transform child in rankingContent)
            {
                Destroy(child.gameObject); // 자식오브젝트 파괴
            }
            
            // 랭킹 매니저에서 최신 데이터 받아오기
            RankingData data = rankingManager.GetRankingData();

            // 랭킹 데이터만큼 반복(10개까지만)
            for (int i = 0; i < data.entries.Count && i < 10; i++)
            {
                RankingEntry entry = data.entries[i]; // i번째 순위 가져오기
                GameObject rankItem = Instantiate(rankItemPrefab, rankingContent); //  프리팹 복제해서 랭킹콘텐츠 자식으로 생성

                // 생성된 랭킹 요소들
                TextMeshProUGUI rankText = rankItem.transform.Find("RankText").GetComponent<TextMeshProUGUI>(); // 랭킹 순위 (숫자)
                TextMeshProUGUI nameText = rankItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>(); // 이름
                TextMeshProUGUI scoreText = rankItem.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>(); // 점수
                TextMeshProUGUI dateText = rankItem.transform.Find("DateText").GetComponent<TextMeshProUGUI>(); // 날짜

                // 각 텍스트에 데이터 입력해주기
                rankText.text = (i + 1).ToString(); // 순위 
                nameText.text = entry.name; // 이름
                scoreText.text = FormatTime(entry.score); // 생존 시간
                dateText.text = entry.date; // 기록한 날짜

                if (i < 3) // 3위 이내일때
                {
                    rankText.fontSize = topRankFontSize; // 랭킹 사이즈
                    nameText.fontSize = topRankFontSize; // 이름 사이즈
                    scoreText.fontSize = topRankFontSize; // 점수 사이즈
                    dateText.fontSize = topRankFontSize - 8; // 날짜 사이즈

                    Color rankColor = topRankColor; // 등수별로 색상 다르게
                    if (i == 0) // 1등
                    {
                        rankColor = new Color(1f, 0.84f, 0f); // 금색
                    }
                    else if (i == 1) // 2등
                    {
                        rankColor = new Color(0.75f, 0.75f, 0.75f); // 은색
                    }
                    else if (i == 2) // 3등
                    {
                        rankColor = new Color(0.8f, 0.5f, 0.2f); // 동색
                    }

                    rankText.color = rankColor;
                    nameText.color = rankColor;
                    scoreText.color = rankColor;
                }
                else
                {
                    rankText.fontSize = normalRankFontSize;
                    nameText.fontSize = normalRankFontSize;
                    scoreText.fontSize = normalRankFontSize;
                    dateText.fontSize = normalRankFontSize - 6;

                    rankText.color = normalRankColor;
                    nameText.color = normalRankColor;
                    scoreText.color = normalRankColor;
                    dateText.color = new Color(0.7f, 0.7f, 0.7f);
                }
            }
        }

        private void OnRestart()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }

        private void OnQuit()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }
    }
}
