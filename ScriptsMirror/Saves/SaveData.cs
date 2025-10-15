using System;

namespace IdleBiz.Saves
{
    [Serializable]
    public class SaveData
    {
        public double money;
        public double lifetime;
        public int gold;
        public int currentBusinessId;
        public long updatedAtUnix;

        // Nauja: visø upgradø lygiai pagal indeksà
        public int[] upgradeLvls;

        // Nauja: uþclaimintø pasiekimø ID
        public string[] claimedAchievements;
    }
}

