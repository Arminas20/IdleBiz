using System;
using UnityEngine;

namespace IdleBiz.Core
{
    public enum UpgradeEffectType { TapFlat, TapMultiplier }

    /// <summary>
    /// Level-based upgrade: kaina = BaseCost * CostMultiplier^Level.
    /// Kiekvienas levelis prideda efektà (pvz., +1 tap).
    /// </summary>
    [Serializable]
    public class UpgradeData
    {
        public string Id;
        public string Name;

        [Header("Cost")]
        public double BaseCost = 10;
        public double CostMultiplier = 1.15;

        [Header("Levels")]
        public int Level = 0;
        public int MaxLevel = 100;

        [Header("Effect per level")]
        public UpgradeEffectType EffectType = UpgradeEffectType.TapFlat;
        public double EffectPerLevel = 1.0; // kiekvienas lvl duoda +1 tap

        public bool IsMax => Level >= MaxLevel;
        public double CurrentCost => IsMax ? double.PositiveInfinity
                                           : BaseCost * Math.Pow(CostMultiplier, Level);
    }
}

