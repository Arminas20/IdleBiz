using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleBiz.Ranking
{
    [Serializable]
    public class RankTier
    {
        public string Name;
        public double RequiredLifetime; // reikalingas lifetime $ norint pasiekti ðá rangà
    }

    [CreateAssetMenu(fileName = "RankConfig", menuName = "IdleBiz/Ranking/Rank Config")]
    public class RankConfig : ScriptableObject
    {
        public List<RankTier> Tiers = new(); // DIDËJANÈIA tvarka pagal RequiredLifetime
    }
}
