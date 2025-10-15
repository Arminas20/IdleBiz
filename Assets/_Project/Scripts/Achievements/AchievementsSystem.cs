using System;
using System.Collections;
using System.Collections.Generic;
using IdleBiz.Core;
using IdleBiz.Ranking; // naudojam RankConfig/RankTier
using UnityEngine;

namespace IdleBiz.Achievements
{
    /// <summary>
    /// Pasiekimai pagal lifetime $ (kaip buvæ "rankai"), bet su atsiëmimo (claim) logika Gold'ui.
    /// </summary>
    public sealed class AchievementsSystem : MonoBehaviour
    {
        public static AchievementsSystem Instance { get; private set; }

        [SerializeField] private RankConfig config;   // pririðk DefaultRankConfig
        public RankConfig Config => config;

        public event Action OnDataChanged;

        public int CurrentIndex { get; private set; }
        public string CurrentName { get; private set; }
        public double CurrentThreshold { get; private set; }
        public string NextName { get; private set; }
        public double NextThreshold { get; private set; }
        public float Progress01 { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        // --- NEW: kuriuos pasiekimus þaidëjas jau atsiëmë (gavo Gold) ---
        [SerializeField] private List<bool> claimed = new(); // indeksas = tier indeksas

        private Coroutine waitCo;
        private bool subscribed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            TrySubscribeOrWait();
            EnsureClaimSize();
            double lt = GameModel.Instance != null ? GameModel.Instance.LifetimeMoney : 0;
            Recompute(lt);
        }

        private void OnDisable()
        {
            if (waitCo != null) { StopCoroutine(waitCo); waitCo = null; }
            if (subscribed && GameModel.Instance != null)
            {
                GameModel.Instance.OnLifetimeChanged -= Recompute;
                subscribed = false;
            }
        }

        private void TrySubscribeOrWait()
        {
            if (GameModel.Instance != null)
            {
                if (!subscribed)
                {
                    GameModel.Instance.OnLifetimeChanged += Recompute;
                    subscribed = true;
                    if (debugLogs) Debug.Log("[AchievementsSystem] Subscribed to GameModel.OnLifetimeChanged.");
                }
            }
            else
            {
                if (waitCo == null) waitCo = StartCoroutine(WaitForGameModelThenSubscribe());
            }
        }

        private IEnumerator WaitForGameModelThenSubscribe()
        {
            if (debugLogs) Debug.Log("[AchievementsSystem] Waiting for GameModel...");
            while (GameModel.Instance == null) yield return null;

            GameModel.Instance.OnLifetimeChanged += Recompute;
            subscribed = true;
            waitCo = null;

            if (debugLogs) Debug.Log("[AchievementsSystem] GameModel found. Subscribed.");
            Recompute(GameModel.Instance.LifetimeMoney);
        }

        public void SetConfig(RankConfig cfg)
        {
            config = cfg;
            EnsureClaimSize();
            Recompute(GameModel.Instance != null ? GameModel.Instance.LifetimeMoney : 0);
        }

        private void EnsureClaimSize()
        {
            int need = (config != null && config.Tiers != null) ? config.Tiers.Count : 0;
            while (claimed.Count < need) claimed.Add(false);
            if (claimed.Count > need && need >= 0) claimed.RemoveRange(need, claimed.Count - need);
        }

        // --- Vieðos uþklausos eilutëms ---
        public bool IsClaimed(int index) => index >= 0 && index < claimed.Count && claimed[index];

        public bool IsUnlocked(int index, double lifetime)
        {
            if (config == null || config.Tiers == null || index < 0 || index >= config.Tiers.Count) return false;
            return lifetime >= config.Tiers[index].RequiredLifetime;
        }

        public bool IsClaimable(int index, double lifetime)
        {
            return IsUnlocked(index, lifetime) && !IsClaimed(index);
        }

        public int GetGoldRewardForIndex(int index)
        {
            // Newbie (index 0) = 1, Self-Starter (1) = 2, ...
            return index + 1;
        }

        public bool TryClaim(int index)
        {
            var gm = GameModel.Instance;
            if (gm == null || config == null || config.Tiers == null) return false;
            EnsureClaimSize();
            if (!IsClaimable(index, gm.LifetimeMoney)) return false;

            claimed[index] = true;
            int reward = GetGoldRewardForIndex(index);
            gm.AddGold(reward);

            if (debugLogs) Debug.Log($"[AchievementsSystem] Claimed '{config.Tiers[index].Name}' ? +{reward}?");

            OnDataChanged?.Invoke();
            return true;
        }

        // --- Skaièiavimai kaip anksèiau ---
        private void Recompute(double lifetime)
        {
            if (config == null || config.Tiers == null || config.Tiers.Count == 0)
            {
                CurrentIndex = 0; CurrentName = "Unranked";
                CurrentThreshold = 0; NextName = "—"; NextThreshold = double.PositiveInfinity;
                Progress01 = 0f;
                OnDataChanged?.Invoke();
                return;
            }

            int idx = 0;
            for (int i = 0; i < config.Tiers.Count; i++)
            {
                if (lifetime >= config.Tiers[i].RequiredLifetime) idx = i;
                else break;
            }

            CurrentIndex = idx;
            CurrentName = config.Tiers[idx].Name;
            CurrentThreshold = config.Tiers[idx].RequiredLifetime;

            if (idx < config.Tiers.Count - 1)
            {
                NextName = config.Tiers[idx + 1].Name;
                NextThreshold = config.Tiers[idx + 1].RequiredLifetime;
                double span = Math.Max(1d, NextThreshold - CurrentThreshold);
                Progress01 = Mathf.Clamp01((float)((lifetime - CurrentThreshold) / span));
            }
            else
            {
                NextName = "MAX";
                NextThreshold = double.PositiveInfinity;
                Progress01 = 1f;
            }

            OnDataChanged?.Invoke();
        }
    }
}
