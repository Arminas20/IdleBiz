using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleBiz.Core;

namespace IdleBiz.UI
{
    public sealed class TapToEarn : MonoBehaviour
    {
        [SerializeField] private Button tapButton;
        [SerializeField] private TMP_Text tapLabel;

        [Header("Config")]
        [SerializeField] private double baseTapReward = 1;

        private void OnEnable()
        {
            tapButton.onClick.AddListener(OnTap);
            if (GameModel.Instance != null)
                GameModel.Instance.OnUpgradesChanged += UpdateLabel;
            UpdateLabel();
        }

        private void OnDisable()
        {
            tapButton.onClick.RemoveListener(OnTap);
            if (GameModel.Instance != null)
                GameModel.Instance.OnUpgradesChanged -= UpdateLabel;
        }

        private void OnTap()
        {
            var gm = GameModel.Instance;
            double reward = gm != null ? gm.ComputeTapReward(baseTapReward) : baseTapReward;
            gm?.AddMoney(reward);
        }

        private void UpdateLabel()
        {
            var gm = GameModel.Instance;
            double reward = gm != null ? gm.ComputeTapReward(baseTapReward) : baseTapReward;
            if (tapLabel != null)
                tapLabel.text = $"Tap +{NumberAbbreviations.Format(reward)}";
        }
    }
}
