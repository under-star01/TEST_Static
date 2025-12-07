using System;
using UnityEngine;

// 네임스페이스로 나눠서 개별관리할 수 있게함
namespace Game.Ranking
{
    [Serializable]
    public class RankingEntry
    {
        public string name; //플레이어이름
        public float score; //생존시간
        public string date; //저장 시간
    }
}
