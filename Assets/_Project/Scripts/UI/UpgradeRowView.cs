using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleBiz.Core;

namespace IdleBiz.UI
{
    /// <summary>
    /// Vienos eilutës vaizdas: Title ? LVL X arba MAX ? Buy(kaina)
    /// </summary>
    public sealed class UpgradeRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;  // TitleText
        [SerializeField] private TMP_Text levelText;  // LevelText
        [SerializeField] private Button buyButton;    // BuyButton
        [SerializeField] private TMP_Text buyLabel;   // BuyButton/BuyLabel

        [Header("Formatting")]
        [SerializeField] private string levelPrefix = "LVL"; // jei norësi "LTL", pakeisk èia

        private UpgradeData data;
        private Func<UpgradeData, bool> onTryBuy;

        public void Setup(UpgradeData d, Func<UpgradeData, bool> tryBuy)
        {
            data = d;
            onTryBuy = tryBuy;
            Refresh();
        }

        private void OnEnable() { if (buyButton) buyButton.onClick.AddListener(OnBuy); }
        private void OnDisable() { if (buyButton) buyButton.onClick.RemoveListener(OnBuy); }

        public void Refresh(double currentMoney = double.NaN)
        {
            if (titleText) titleText.text = data.Name;

            if (levelText)
                levelText.text = data.IsMax ? "MAX" : $"{levelPrefix}{data.Level}";

            if (data.IsMax)
            {
                if (buyButton) buyButton.interactable = false;
                if (buyLabel) buyLabel.text = "Max";
                return;
            }

            if (!double.IsNaN(currentMoney) && buyButton)
                buyButton.interactable = currentMoney >= data.CurrentCost;

            if (buyLabel)
                buyLabel.text = $"Buy ({NumberAbbreviations.Format(data.CurrentCost)})";
        }

        private void OnBuy()
        {
            if (data.IsMax) return;
            bool ok = onTryBuy?.Invoke(data) ?? false;
            if (ok) Refresh();
        }
    }
}
