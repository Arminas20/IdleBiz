using System.Collections.Generic;
using IdleBiz.Business;
using IdleBiz.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleBiz.UI
{
    /// <summary>
    /// Kuria „Business“ sàraðà ir valdo Unlock/Go.
    /// </summary>
    public sealed class BusinessPanelController : MonoBehaviour
    {
        [Header("Top")]
        [SerializeField] private Button backButton;
        [SerializeField] private TabController tabs;

        [Header("List")]
        [SerializeField] private Transform listContent;     // Scroll/Viewport/Content
        [SerializeField] private GameObject rowPrefab;      // BusinessRow prefabas

        private readonly List<(BusinessDef def, BusinessRowView view)> rows = new();

        private void OnEnable()
        {
            if (backButton && tabs) backButton.onClick.AddListener(tabs.ShowMain);

            if (BusinessSystem.Instance != null)
            {
                BusinessSystem.Instance.OnListChanged += Rebuild;
                BusinessSystem.Instance.OnCurrentChanged += _ => Refresh();
            }
            if (GameModel.Instance != null)
            {
                GameModel.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            Rebuild();
        }

        private void OnDisable()
        {
            if (backButton && tabs) backButton.onClick.RemoveListener(tabs.ShowMain);

            if (BusinessSystem.Instance != null)
            {
                BusinessSystem.Instance.OnListChanged -= Rebuild;
                BusinessSystem.Instance.OnCurrentChanged -= _ => Refresh();
            }
            if (GameModel.Instance != null)
            {
                GameModel.Instance.OnMoneyChanged -= OnMoneyChanged;
            }
        }

        private void OnMoneyChanged(double money)
        {
            foreach (var t in rows)
                t.view.RefreshMoney(money);
        }

        private void Rebuild()
        {
            if (listContent == null || rowPrefab == null)
            {
                Debug.LogError("[BusinessPanel] listContent arba rowPrefab nepririðtas.");
                return;
            }

            // Iðvalom
            for (int i = listContent.childCount - 1; i >= 0; i--)
                Destroy(listContent.GetChild(i).gameObject);
            rows.Clear();

            var sys = BusinessSystem.Instance;
            var gm = GameModel.Instance;
            if (sys == null || gm == null || sys.Catalog == null || sys.Catalog.Items == null) return;

            foreach (var def in sys.All())
            {
                var go = Instantiate(rowPrefab, listContent);
                var view = go.GetComponent<BusinessRowView>();
                if (view == null)
                {
                    Debug.LogError("[BusinessPanel] BusinessRowView nerastas ant prefabo.");
                    continue;
                }

                // Uþfiksuojam lokalø kintamàjá lamdai
                var defLocal = def;

                view.Setup(
                    d: defLocal,
                    unlocked: sys.IsUnlocked(defLocal.Id),
                    isCurrent: sys.IsCurrent(defLocal.Id),
                    playerMoney: gm.Money,
                    onTryUnlock: () => sys.TryUnlock(defLocal.Id),
                    onGo: () => sys.GoTo(defLocal.Id)
                );

                rows.Add((defLocal, view));
            }
        }

        private void Refresh()
        {
            var sys = BusinessSystem.Instance;
            var gm = GameModel.Instance;
            if (sys == null || gm == null) return;

            foreach (var t in rows)
            {
                t.view.Setup(
                    d: t.def,
                    unlocked: sys.IsUnlocked(t.def.Id),
                    isCurrent: sys.IsCurrent(t.def.Id),
                    playerMoney: gm.Money,
                    onTryUnlock: () => sys.TryUnlock(t.def.Id),
                    onGo: () => sys.GoTo(t.def.Id)
                );
            }
        }
    }
}
