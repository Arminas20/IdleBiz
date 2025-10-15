using System;
using IdleBiz.Business;
using IdleBiz.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleBiz.UI
{
    /// <summary>
    /// Eilutë: Title | Status/Price | Unlock/Go/Current
    /// </summary>
    public sealed class BusinessRowView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;     // „Freelancer“
        [SerializeField] private TMP_Text statusText;    // „Unlocked“ arba „$20K“
        [Header("Buttons")]
        [SerializeField] private Button unlockButton;    // rodom, jei locked
        [SerializeField] private TMP_Text unlockLabel;   // „Unlock“
        [SerializeField] private Button goButton;        // rodom, jei unlocked
        [SerializeField] private TMP_Text goLabel;       // „Go“ arba „Current“

        private BusinessDef def;
        private Func<bool> tryUnlock;
        private Action goTo;

        private void OnEnable()
        {
            if (unlockButton) { unlockButton.onClick.RemoveAllListeners(); unlockButton.onClick.AddListener(OnUnlockPressed); }
            if (goButton) { goButton.onClick.RemoveAllListeners(); goButton.onClick.AddListener(OnGoPressed); }
        }

        public void Setup(BusinessDef d, bool unlocked, bool isCurrent, double playerMoney, Func<bool> onTryUnlock, Action onGo)
        {
            def = d;
            tryUnlock = onTryUnlock;
            goTo = onGo;

            if (titleText) titleText.text = d.Name;

            if (!unlocked)
            {
                if (statusText) statusText.text = "$" + NumberAbbreviations.Format(d.UnlockCost);
                if (unlockButton) { unlockButton.gameObject.SetActive(true); unlockButton.interactable = playerMoney >= d.UnlockCost; }
                if (unlockLabel) unlockLabel.text = "Unlock";
                if (goButton) goButton.gameObject.SetActive(false);
            }
            else
            {
                if (statusText) statusText.text = "Unlocked";
                if (unlockButton) unlockButton.gameObject.SetActive(false);
                if (goButton)
                {
                    goButton.gameObject.SetActive(true);
                    goButton.interactable = !isCurrent;
                }
                if (goLabel) goLabel.text = isCurrent ? "Current" : "Go";
            }
        }

        public void RefreshMoney(double playerMoney)
        {
            if (def == null) return;
            // jei row ðiuo metu „locked“ rodomas – atnaujinam Unlock interaktyvumà
            if (unlockButton && unlockButton.gameObject.activeSelf && statusText)
            {
                unlockButton.interactable = playerMoney >= def.UnlockCost;
            }
        }

        private void OnUnlockPressed()
        {
            if (tryUnlock?.Invoke() == true)
            {
                // BusinessPanel per-build’ins sàraðà per OnListChanged, èia nieko daugiau nereikia
            }
        }

        private void OnGoPressed() => goTo?.Invoke();
    }
}
