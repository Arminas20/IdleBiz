using TMPro;
using UnityEngine;
using IdleBiz.Core;

namespace IdleBiz.UI
{
    public sealed class UIBootstrap : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyLabelText;
        [SerializeField] private string currencyLabel = "Money";

        private void Awake()
        {
            if (GameModel.Instance == null)
            {
                var go = new GameObject("GameModel");
                go.AddComponent<GameModel>();
            }
        }

        private void Start()
        {
            if (moneyLabelText != null)
                moneyLabelText.text = currencyLabel + ":";
        }
    }
}
