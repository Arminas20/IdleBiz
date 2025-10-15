using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleBiz.Core;

namespace IdleBiz.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class UnlockGate : MonoBehaviour
    {
        [SerializeField] private double requiredMoney = 50;     // gali keisti Inspector’iuje
        [SerializeField] private TMP_Text label;                 // mygtuko vidinis TMP
        [SerializeField] private string baseText = "Bussines";   // palieku kaip tavo pavadinimas

        private Button btn;

        private void Start()
        {
            btn = GetComponent<Button>();
            if (GameModel.Instance != null)
            {
                GameModel.Instance.OnMoneyChanged += OnMoneyChanged;
                Refresh(GameModel.Instance.Money);
            }
            else
            {
                // jei dël kokios prieþasties GameModel dar neparuoðtas
                btn.interactable = false;
                UpdateLabelLocked();
            }
        }

        private void OnDestroy()
        {
            if (GameModel.Instance != null)
                GameModel.Instance.OnMoneyChanged -= OnMoneyChanged;
        }

        private void OnMoneyChanged(double money) => Refresh(money);

        private void Refresh(double money)
        {
            bool unlocked = money >= requiredMoney;
            btn.interactable = unlocked;
            if (label != null)
                label.text = unlocked ? baseText : $"{baseText} ({NumberAbbreviations.Format(requiredMoney)})";
        }

        private void UpdateLabelLocked()
        {
            if (label != null) label.text = $"{baseText} ({NumberAbbreviations.Format(requiredMoney)})";
        }
    }
}

