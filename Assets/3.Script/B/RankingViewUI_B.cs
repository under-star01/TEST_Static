using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Ranking;

namespace Game.UI
{
    public class RankingViewUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject rankingViewPanel;
        [SerializeField] private Transform rankingContent;
        [SerializeField] private GameObject rankItemPrefab;
        [SerializeField] private Button closeButton;

        [Header("Settings")]
        [SerializeField] private int topRankFontSize = 32;
        [SerializeField] private int normalRankFontSize = 24;
        [SerializeField] private Color topRankColor = Color.yellow;
        [SerializeField] private Color normalRankColor = Color.white;

        private RankingManager rankingManager;
        private List<GameObject> rankingItems = new List<GameObject>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            rankingManager = FindAnyObjectByType<RankingManager>();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseRankingView);
            }

            rankingViewPanel.SetActive(false);
        }

        public void OpenRankingView()
        {
            AudioManager.Instance.PlayButtonSFX();
            rankingViewPanel.SetActive(true);
            DisplayRanking();
            
            Time.timeScale = 0f;
        }

        public void CloseRankingView()
        {
            AudioManager.Instance.PlayButtonSFX();
            rankingViewPanel.SetActive(false);

            Time.timeScale = 1f;
        }

        private void DisplayRanking()
        {
            if (rankingManager == null)
            {
                Debug.LogError("RankingManager not found!");
                return;
            }

            RankingData data = rankingManager.GetRankingData();
            int rankCount = Mathf.Min(data.entries.Count, 10);

            while (rankingItems.Count < rankCount)
            {
                GameObject newItem = Instantiate(rankItemPrefab, rankingContent);
                rankingItems.Add(newItem);
            }

            for (int i = 0; i < rankingItems.Count; i++)
            {
                if (i < rankCount)
                {
                    rankingItems[i].SetActive(true);
                    RankingEntry entry = data.entries[i];
                    UpdateRankItem(rankingItems[i], i, entry);
                }
                else
                {
                    rankingItems[i].SetActive(false);
                }
            }

            if (rankCount == 0)
            {
                Debug.Log("No ranking data available");
            }
        }

        private void UpdateRankItem(GameObject rankItem, int index, RankingEntry entry)
        {
            TextMeshProUGUI rankText = rankItem.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = rankItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = rankItem.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = rankItem.transform.Find("DateText").GetComponent<TextMeshProUGUI>();

            rankText.text = (index + 1).ToString();
            nameText.text = entry.name;
            scoreText.text = FormatTime(entry.score);
            dateText.text = entry.date;

            if (index < 3)
            {
                rankText.fontSize = topRankFontSize;
                nameText.fontSize = topRankFontSize;
                scoreText.fontSize = topRankFontSize;
                dateText.fontSize = topRankFontSize - 8;

                Color rankColor = topRankColor;
                if (index == 0)
                {
                    rankColor = new Color(1f, 0.84f, 0f);
                }
                else if (index == 1)
                {
                    rankColor = new Color(0.75f, 0.75f, 0.75f);
                }
                else if (index == 2)
                {
                    rankColor = new Color(0.8f, 0.5f, 0.2f);
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

        private string FormatTime(float timeInSeconds)
        {
            int minutes = (int)(timeInSeconds / 60f);
            int seconds = (int)(timeInSeconds % 60f);
            int milliseconds = (int)((timeInSeconds * 100f) % 100f);

            return minutes.ToString("00") + ":" + seconds.ToString("00") + "." + milliseconds.ToString("00");
        }

        void OnDestroy()
        {
            foreach (GameObject item in rankingItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            rankingItems.Clear();
        }
    }
}
