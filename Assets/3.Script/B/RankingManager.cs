using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Ranking
{
    //RankingData 및 Entry 사용하는 매니저
    public class RankingManager : MonoBehaviour
    {
        /*전체적인 흐름
         게임 시작 시: 저장된 랭킹 로드
        게임 오버 시: 생존 시간 계산, 새 엔트리 생성 후 랭킹에 추가
        기존 랭킹에서 점수 기준으로 내림차순 정렬
        10위까지 잘라 저장
        필요 시 이름 입력 UI 또는 자동 이름 부여
        화면에 랭킹 표시를 원하면 UI 업데이트 함수 추가
         */

        [SerializeField]
        private string fileName = "ranking.json"; // 랭킹 저장 JSON 파일명 -임의지정(변경가능)
        private string FilePath;

        private RankingData rankingData = new RankingData(); //위에서 만든 랭킹 저장하는 리스트 불러오기

        void Awake()
        {
            FilePath = Path.Combine(Application.persistentDataPath, fileName); //저장경로 설정(임의지정상태)
            LoadRanking(); //어웨이크 단계에서 저장되어있는 JSON 호출
        }

        // 랭킹 데이터를 외부에서(UI등) 조회가능하도록 반환
        public RankingData GetRankingData()
        {
            return rankingData; //랭킹 리스트
        }

        // 게임 오버 시 생존 시간으로 랭킹에 새로운 항목 추가 및 저장
        public void SubmitScore(string playerName, float score) //이름, 점수 저장
        {
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            RankingEntry newEntry = new RankingEntry { name = playerName, score = score, date = dateStr };

            rankingData.entries.Add(newEntry);
            // 내림차순으로 정렬하고 상위 10개만 유지
            rankingData.entries.Sort((a, b) => b.score.CompareTo(a.score)); // 내림차순 정렬할게요
                                                                            //로직 : 랭킹 리스트 정렬 (a,b 비교) => b가 a보다 크면 양수 반환 > 앞으로 옴(내림차순)


            if (rankingData.entries.Count > 10) //10개까지만 엔트리에 담을거에요
            {
                rankingData.entries = rankingData.entries.GetRange(0, 10); //0부터 10까지(미만) 데이터만 엔트리함.
            }

            SaveRanking(); //저장하는 메소드 호출
        }

        // 저장
        private void SaveRanking()
        {
            try
            {
                // JSON에 스트링 형태로 랭킹 데이터를 저장합니다.
                //prettyPrint - 들여쓰기, 줄바꿈 등을 적용해서 보기 좋게 생성(보기좋게 만든다해서 프리티프린트)
                string json = JsonUtility.ToJson(rankingData, true);
                // JSON을 지정경로(filepath)에 저장하는데, 기존 파일이 있다면 덮어쓰기함-namespace에 system.io.file 사용해야함
                File.WriteAllText(FilePath, json);
                // 저장하고나서 지정경로 어디에 저장 성공함! 하고 알려줌
                Debug.Log("Ranking saved to: " + FilePath);
            }
            //위에 try문에서 실패하면 알려줌(명령어대로 시도(try)하고, 안된거 캐치해서 알려주는거임)
            catch (Exception ex)
            {
                Debug.LogError("Failed to save ranking: " + ex);//저장에 실패했다고 빨간에러로 알려줌)
            }
        }

        // 로드
        private void LoadRanking()
        {
            try
            {
                // 지정 경로에 파일이 존재하는지 체크
                if (File.Exists(FilePath))
                {
                    // 지정 경로의 JSON 파일을 불러와 json 스트링에 저장
                    string json = File.ReadAllText(FilePath);
                    // JSON 파일의 문자열을 C#에서 읽을 수 있게 역직렬화(번역)
                    rankingData = JsonUtility.FromJson<RankingData>(json);
                    // 데이터가 비어있거나, 목록이 비어있으면(또는 파일이 깨져서 못읽을때..?)
                    // 새롭게 명단 하나 만들어!(초기화)
                    if (rankingData == null || rankingData.entries == null)
                    {
                        rankingData = new RankingData();
                    }
                }
                // 지정 경로에 파일이 없을때겠지?
                else
                {
                    //이때도 새로 만들억!(초기화)
                    rankingData = new RankingData();
                }
            }
            catch (Exception ex)
            {
                // 파일 불러오는데 실패했다고?
                Debug.LogError("Failed to load ranking: " + ex);
                // 새로 만들라!!
                rankingData = new RankingData();
            }
        }

        // 테스트용으로 랭크를 출력할때 사용
        public void PrintRanking()
        {
            // 랭킹데이터 엔트리(명단) 안에 있는 값에 접근하는데, e라는 이름으로 임시 할당해!
            foreach (var e in rankingData.entries)
            {
                // 콘솔에 로그 띄워서 테스트해. 이름, 점수, 날짜순이야
                Debug.Log($"Name: {e.name}, Score: {e.score}, Date: {e.date}");
            }
        }
    }
}
