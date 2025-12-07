using System;
using System.Collections.Generic;

// 네임스페이스로 나눠서 개별관리할 수 있게함
namespace Game.Ranking
{
    [Serializable]
    public class RankingData
    {
        // 리스트 직렬화하기 위해 public
        public List<RankingEntry> entries = new List<RankingEntry>();
    }
}
