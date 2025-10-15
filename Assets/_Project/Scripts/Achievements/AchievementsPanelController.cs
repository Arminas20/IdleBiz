using IdleBiz.Core;
using IdleBiz.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace IdleBiz.Achievements
{
    public sealed class AchievementsPanelController : MonoBehaviour
    {
        [Header("Top")]
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private Button backButton;
        [SerializeField] private TabController tabs;

        [Header("Current Card")]
        [SerializeField] private TMP_Text activeAchText;
        [SerializeField] private TMP_Text lifetimeText;
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text nextAchText;

        [Header("List")]
        [SerializeField] private Transform listContent;            // ScrollRect/Viewport/Content
        [SerializeField] private GameObject achievementRowPrefab;  // Prefabas su AchievementRowView

        [Header("Refs (drag & drop)")]
        [SerializeField] private AchievementsSystem system;        // <- PRIRIÐK INSPECTORIUJE

        [Header("Debug")]
        [SerializeField] private bool debugLogs = true;

        private AchievementsPersistenceAdapter _adapter;
        private AchievementsSystem _sys;
        private bool _subscribed;

        private void OnEnable()
        {
            if (backButton && tabs) backButton.onClick.AddListener(tabs.ShowMain);

            _adapter = GetComponent<AchievementsPersistenceAdapter>();
            IdleBiz.Saves.SaveOrchestrator.Instance?.RegisterAchievementsProvider(_adapter);

            // naudok, kas pririðta Inspector’iuje; jei nëra — fallback á paieðkà
            _sys = system != null ? system : AchievementsSystem.Instance;
            if (_sys == null)
            {
#if UNITY_2022_3_OR_NEWER || UNITY_6000_0_OR_NEWER
                _sys = FindFirstObjectByType<AchievementsSystem>(FindObjectsInactive.Include);
#else
                _sys = FindObjectOfType<AchievementsSystem>();
#endif
            }

            StartCoroutine(EnsureSystemAndBuild());
        }

        private void OnDisable()
        {
            if (backButton && tabs) backButton.onClick.RemoveListener(tabs.ShowMain);

            if (_subscribed && _sys != null)
            {
                _sys.OnDataChanged -= RefreshAll;
                _subscribed = false;
            }

            if (GameModel.Instance != null)
                GameModel.Instance.OnLifetimeChanged -= OnLifetimeChanged;
        }

        private IEnumerator EnsureSystemAndBuild()
        {
            // jeigu sistemos vis dar nëra, palaukiam kelis kadrus
            int frames = 0;
            while (_sys == null && frames < 10) { frames++; yield return null; }

            if (_sys == null)
            {
                if (debugLogs) Debug.LogWarning("[AchievementsPanel] System reference is missing.");
                if (headerText) headerText.text = "Achievements";
                yield break;
            }

            if (!_subscribed)
            {
                _sys.OnDataChanged += RefreshAll;
                _subscribed = true;
            }

            if (GameModel.Instance != null)
                GameModel.Instance.OnLifetimeChanged += OnLifetimeChanged;

            BuildList();
            RefreshUI();
        }

        private void OnLifetimeChanged(double _) => RefreshUI();

        private static string MakeId(string name) =>
            string.IsNullOrEmpty(name) ? "" : new string(name.ToLower().Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());

        private void RefreshAll()
        {
            BuildList();
            RefreshUI();
        }

        private void BuildList()
        {
            if (!listContent || !achievementRowPrefab) return;
            if (_sys == null || _sys.Config == null || _sys.Config.Tiers == null || _sys.Config.Tiers.Count == 0) return;

            var contentRT = listContent as RectTransform;
            var vlg = listContent.GetComponent<VerticalLayoutGroup>() ?? listContent.gameObject.AddComponent<VerticalLayoutGroup>();
            var csf = listContent.GetComponent<ContentSizeFitter>() ?? listContent.gameObject.AddComponent<ContentSizeFitter>();
            vlg.childControlWidth = vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            vlg.spacing = 12f;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = listContent.childCount - 1; i >= 0; i--)
                Destroy(listContent.GetChild(i).gameObject);

            double lifetime = GameModel.Instance ? GameModel.Instance.LifetimeMoney : 0;

            HashSet<string> claimedSet = new();
            if (_adapter != null && _adapter.Claimed != null)
                for (int i = 0; i < _adapter.Claimed.Count; i++)
                    if (!string.IsNullOrEmpty(_adapter.Claimed[i])) claimedSet.Add(_adapter.Claimed[i]);

            int created = 0;
            for (int i = 0; i < _sys.Config.Tiers.Count; i++)
            {
                var tier = _sys.Config.Tiers[i];
                var id = MakeId(tier.Name);

                bool isUnlocked = _sys.IsUnlocked(i, lifetime);
                bool isClaimed = _sys.IsClaimed(i) || claimedSet.Contains(id);
                bool claimable = _sys.IsClaimable(i, lifetime) && !isClaimed;
                bool isCurrent = i == _sys.CurrentIndex;

                string status =
                    claimable ? "" :
                    isClaimed ? "Completed" :
                    isCurrent ? "Active" :
                    isUnlocked ? "Completed" : "Locked";

                int reward = _sys.GetGoldRewardForIndex(i);

                var rowGO = Instantiate(achievementRowPrefab);
                rowGO.transform.SetParent(listContent, false);
                rowGO.SetActive(true);

                var row = rowGO.GetComponent<AchievementRowView>();
                if (row == null) continue;

                int idx = i;
                row.Setup(
                    tier.Name,
                    "$" + NumberAbbreviations.Format(tier.RequiredLifetime),
                    status,
                    claimable,
                    reward,
                    isCurrent,
                    onClaimAction: () =>
                    {
                        if (claimedSet.Contains(id)) return;
                        if (_sys.TryClaim(idx))
                        {
                            _adapter?.MarkClaimed(id);
                            claimedSet.Add(id);
                            IdleBiz.Saves.SaveOrchestrator.Instance?.SaveNow();
                            row.Setup(tier.Name, "$" + NumberAbbreviations.Format(tier.RequiredLifetime),
                                      "Completed", false, reward, isCurrent, null);
                        }
                    }
                );
                created++;
            }

            if (debugLogs) Debug.Log($"[AchievementsPanel] Created rows: {created}");

            if (contentRT != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
                StartCoroutine(RebuildNextFrame(contentRT));
            }
        }

        private IEnumerator RebuildNextFrame(RectTransform rt)
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }

        private void RefreshUI()
        {
            var gm = GameModel.Instance;
            if (_sys == null || gm == null) return;

            if (activeAchText) activeAchText.text = $"Active: {_sys.CurrentName}";
            if (lifetimeText) lifetimeText.text = $"Lifetime: ${NumberAbbreviations.Format(gm.LifetimeMoney)}";

            float fill = 1f;
            if (_sys.NextName != "MAX")
            {
                float cur = (float)_sys.CurrentThreshold;
                float next = (float)_sys.NextThreshold;
                float span = Mathf.Max(1f, next - cur);
                float val = Mathf.Clamp((float)gm.LifetimeMoney - cur, 0f, span);
                fill = (span > 0f) ? (val / span) : 1f;
            }
            if (progressFill) progressFill.fillAmount = Mathf.Clamp01(fill);

            if (nextAchText)
            {
                nextAchText.text = (_sys.NextName == "MAX")
                    ? "All achievements completed"
                    : $"Next: {_sys.NextName} (${NumberAbbreviations.Format(_sys.NextThreshold)})";
            }
        }
    }
}
