using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleBiz.Achievements
{
    /// <summary>
    /// Eilutė: Pavadinimas | Slenkstis | Statusas ARBA [Claim N★] mygtukas.
    /// </summary>
    public sealed class AchievementRowView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text thresholdText;
        [SerializeField] private TMP_Text statusText;   // "Completed" | "Active" | "Locked"

        [Header("Claim UI")]
        [SerializeField] private Button claimButton;  // jei claimable = true, rodom šį
        [SerializeField] private TMP_Text claimLabel;   // "Claim 2★"

        private Action onClaim;

        private void OnEnable()
        {
            if (claimButton) claimButton.onClick.AddListener(HandleClaim);
        }

        private void OnDisable()
        {
            if (claimButton) claimButton.onClick.RemoveListener(HandleClaim);
        }

        private void HandleClaim() => onClaim?.Invoke();

        /// <param name="status">"Completed" | "Active" | "Locked" (naudojamas kai claimable=false)</param>
        /// <param name="claimable">jei true – slepiam statusText ir rodom Claim mygtuką</param>
        /// <param name="rewardStars">kiek ★ duos, rodom ant mygtuko</param>
        /// <param name="highlight">paryškinam Active eilutę</param>
        /// <param name="onClaimAction">iškviečiam paspaudus Claim</param>
        public void Setup(string name, string threshold, string status, bool claimable, int rewardStars, bool highlight, Action onClaimAction)
        {
            if (titleText) titleText.text = name;
            if (thresholdText) thresholdText.text = threshold;

            onClaim = onClaimAction;

            if (claimable)
            {
                if (statusText) statusText.gameObject.SetActive(false);
                if (claimButton) claimButton.gameObject.SetActive(true);
                if (claimLabel) claimLabel.text = $"Claim {rewardStars}G";
                if (claimButton) claimButton.interactable = true;
            }
            else
            {
                if (claimButton) claimButton.gameObject.SetActive(false);
                if (statusText)
                {
                    statusText.gameObject.SetActive(true);
                    statusText.text = status;
                    statusText.fontStyle = highlight ? FontStyles.Bold : FontStyles.Normal;
                }
            }
        }
    }
}

