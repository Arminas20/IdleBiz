using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IdleBiz.Core;
using System.Linq;

namespace IdleBiz.UI
{
    public sealed class UpgradesSystem : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private Transform listContent;
        [SerializeField] private GameObject upgradeRowPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private TabController tabs;

        private readonly List<UpgradeData> upgrades = new();
        private readonly List<UpgradeRowView> rows = new();
        private bool built;

        // >>> NAUJA: įvykis, kai nupirktas bet koks lygis
        public event System.Action OnAnyUpgradePurchased;

        public IReadOnlyList<UpgradeData> AllUpgrades => upgrades;

        private void Awake()
        {
            // (tavo esami upgradų aprašai – palieku kaip yra)
            upgrades.Add(new UpgradeData { Id = "healthy_diet", Name = "Healthy Diet", BaseCost = 10, CostMultiplier = 1.15, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 1 });
            upgrades.Add(new UpgradeData { Id = "exercise", Name = "Exercise", BaseCost = 50, CostMultiplier = 1.17, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 1 });
            upgrades.Add(new UpgradeData { Id = "meditation", Name = "Meditation", BaseCost = 150, CostMultiplier = 1.20, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 1 });

            upgrades.Add(new UpgradeData { Id = "focus_timer", Name = "Focus Timer Pro", BaseCost = 800, CostMultiplier = 1.28, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 2 });
            upgrades.Add(new UpgradeData { Id = "typing_practice", Name = "Typing Practice", BaseCost = 2000, CostMultiplier = 1.30, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 2 });
            upgrades.Add(new UpgradeData { Id = "productivity_suite", Name = "Productivity Suite", BaseCost = 6000, CostMultiplier = 1.32, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 2 });

            upgrades.Add(new UpgradeData { Id = "deep_work", Name = "Deep Work Mastery", BaseCost = 15000, CostMultiplier = 1.36, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 3 });
            upgrades.Add(new UpgradeData { Id = "automation_toolkit", Name = "Automation Toolkit", BaseCost = 40000, CostMultiplier = 1.38, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 3 });
            upgrades.Add(new UpgradeData { Id = "client_network_pro", Name = "Client Network Pro", BaseCost = 120000, CostMultiplier = 1.42, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 3 });

            upgrades.Add(new UpgradeData { Id = "ai_assistant_pro", Name = "AI Assistant Pro", BaseCost = 300000, CostMultiplier = 1.46, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 4 });
            upgrades.Add(new UpgradeData { Id = "brand_authority", Name = "Brand Authority", BaseCost = 900000, CostMultiplier = 1.48, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 4 });
            upgrades.Add(new UpgradeData { Id = "agency_ops_os", Name = "Agency Ops OS", BaseCost = 2500000, CostMultiplier = 1.52, MaxLevel = 100, EffectType = UpgradeEffectType.TapFlat, EffectPerLevel = 4 });

            // Registruojamės į orkestrą kuo anksčiau
            if (IdleBiz.Saves.SaveOrchestrator.Instance != null)
                IdleBiz.Saves.SaveOrchestrator.Instance.RegisterUpgradesSystem(this);
        }

        private void OnEnable()
        {
            if (!built) BuildList();

            if (backButton && tabs) backButton.onClick.AddListener(tabs.ShowMain);
            if (GameModel.Instance != null)
                GameModel.Instance.OnMoneyChanged += OnMoneyChanged;

            // jei scenai pakraunant UpgradesSystem atsirado vėliau – dar kartą užregistruojam
            if (IdleBiz.Saves.SaveOrchestrator.Instance != null)
                IdleBiz.Saves.SaveOrchestrator.Instance.RegisterUpgradesSystem(this);

            OnMoneyChanged(GameModel.Instance != null ? GameModel.Instance.Money : 0);
        }

        private void OnDisable()
        {
            if (backButton && tabs) backButton.onClick.RemoveListener(tabs.ShowMain);
            if (GameModel.Instance != null)
                GameModel.Instance.OnMoneyChanged -= OnMoneyChanged;
        }

        private void BuildList()
        {
            if (!listContent || !upgradeRowPrefab)
            {
                Debug.LogError("[UpgradesSystem] Missing listContent or upgradeRowPrefab.");
                return;
            }

            for (int i = listContent.childCount - 1; i >= 0; i--)
                Destroy(listContent.GetChild(i).gameObject);

            rows.Clear();
            foreach (var up in upgrades)
            {
                var go = Instantiate(upgradeRowPrefab, listContent);
                var view = go.GetComponent<UpgradeRowView>();
                if (!view)
                {
                    Debug.LogError("[UpgradesSystem] UpgradeRowView missing on prefab.");
                    continue;
                }
                view.Setup(up, TryBuyOneLevel);
                rows.Add(view);
            }

            built = true;
            RebuildTapBonuses();
        }

        private void OnMoneyChanged(double money)
        {
            foreach (var r in rows) r.Refresh(money);
        }

        private bool TryBuyOneLevel(UpgradeData u)
        {
            var gm = GameModel.Instance;
            if (gm == null || u.IsMax) return false;

            var cost = u.CurrentCost;
            if (!gm.TrySpend(cost)) return false;

            u.Level++;
            switch (u.EffectType)
            {
                case UpgradeEffectType.TapFlat: gm.AddTapFlat(u.EffectPerLevel); break;
                case UpgradeEffectType.TapMultiplier: gm.MultiplyTap(u.EffectPerLevel); break;
            }

            OnAnyUpgradePurchased?.Invoke(); // >>> SAUGOM PO PIRKIMO
            return true;
        }

        // ==== API IŠSAUGOJIMUI
        public int[] CollectLevels() => upgrades.Select(u => u.Level).ToArray();

        public void ApplyLevels(int[] levels)
        {
            if (levels == null || levels.Length == 0) return;
            int n = Mathf.Min(levels.Length, upgrades.Count);
            for (int i = 0; i < n; i++)
                upgrades[i].Level = Mathf.Clamp(levels[i], 0, upgrades[i].MaxLevel);

            RebuildTapBonuses();
            OnMoneyChanged(GameModel.Instance != null ? GameModel.Instance.Money : 0);
        }

        public void RebuildTapBonuses()
        {
            var gm = GameModel.Instance;
            if (gm == null) return;

            gm.ResetTapBonuses();
            foreach (var u in upgrades)
            {
                if (u.Level <= 0) continue;
                switch (u.EffectType)
                {
                    case UpgradeEffectType.TapFlat:
                        gm.AddTapFlat(u.EffectPerLevel * u.Level);
                        break;
                    case UpgradeEffectType.TapMultiplier:
                        for (int k = 0; k < u.Level; k++) gm.MultiplyTap(u.EffectPerLevel);
                        break;
                }
            }
        }
    }
}
