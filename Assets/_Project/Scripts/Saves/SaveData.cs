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

        // Nauja: vis� upgrad� lygiai pagal indeks�
        public int[] upgradeLvls;

        // Nauja: u�claimint� pasiekim� ID
        public string[] claimedAchievements;
    }
}

