using System;
using System.Collections.Generic;
using IdleBiz.Core;
using UnityEngine;

namespace IdleBiz.Business
{
    /// <summary>
    /// Tvarko verslø atrakinimà ir „current“ pasirinkimà.
    /// </summary>
    public sealed class BusinessSystem : MonoBehaviour
    {
        public static BusinessSystem Instance { get; private set; }

        [SerializeField] private BusinessCatalog catalog;  // PRIRIÐK Inspector'iuje
        public BusinessCatalog Catalog => catalog;

        public event Action OnListChanged;               // kai atsirakina ar persijungia
        public event Action<string> OnCurrentChanged;    // praneða naujà current id

        [SerializeField] private string currentBusinessId;
        private readonly HashSet<string> unlocked = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() => InitializeFromCatalog();
        private void OnDisable() { /* rezervuota ateièiai (persist/Save) */ }

        private void InitializeFromCatalog()
        {
            unlocked.Clear();
            if (catalog == null || catalog.Items == null) return;

            foreach (var b in catalog.Items)
                if (b.UnlockedByDefault) unlocked.Add(b.Id);

            // jei current neástatytas arba neatitinka unlocked — pastatom á pirmà unlocked ar tiesiog pirmà sàraðe
            if (string.IsNullOrEmpty(currentBusinessId) || !IsUnlocked(currentBusinessId))
            {
                var firstUnlocked = catalog.Items.Find(b => b.UnlockedByDefault) ?? (catalog.Items.Count > 0 ? catalog.Items[0] : null);
                currentBusinessId = firstUnlocked != null ? firstUnlocked.Id : null;
            }

            OnListChanged?.Invoke();
            if (!string.IsNullOrEmpty(currentBusinessId)) OnCurrentChanged?.Invoke(currentBusinessId);
        }

        public IEnumerable<BusinessDef> All()
        {
            if (catalog?.Items == null) yield break;
            foreach (var b in catalog.Items) yield return b;
        }

        public BusinessDef Find(string id) => catalog?.Items?.Find(b => b.Id == id);
        public bool IsUnlocked(string id) => !string.IsNullOrEmpty(id) && unlocked.Contains(id);
        public bool IsCurrent(string id) => !string.IsNullOrEmpty(id) && id == currentBusinessId;
        public string CurrentId => currentBusinessId;

        public bool TryUnlock(string id)
        {
            var def = Find(id);
            if (def == null) return false;
            if (IsUnlocked(id)) return true;

            var gm = GameModel.Instance;
            if (gm == null) return false;
            if (!gm.TrySpend(def.UnlockCost)) return false;

            unlocked.Add(id);
            OnListChanged?.Invoke();
            return true;
        }

        public void GoTo(string id)
        {
            if (!IsUnlocked(id)) return;
            if (currentBusinessId == id) return;
            currentBusinessId = id;
            OnListChanged?.Invoke();
            OnCurrentChanged?.Invoke(id);
        }
    }
}

