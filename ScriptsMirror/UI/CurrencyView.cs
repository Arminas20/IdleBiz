using System.Collections;
using IdleBiz.Core;
using TMPro;
using UnityEngine;

namespace IdleBiz.UI
{
    /// <summary>
    /// Atvaizduoja $ ir Gold (prestige) sumas TopBar'e.
    /// Atspari versija: jei GameModel dar nesukurtas, palaukia ir tada prisiregistruoja.
    /// </summary>
    public sealed class CurrencyView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text moneyValueText; // $…
        [SerializeField] private TMP_Text goldValueText;  // ★ …

        private bool subscribed;
        private Coroutine waitCo;

        private void OnEnable()
        {
            TrySubscribeOrWait();

            // pradinis užpildymas (jei GM dar nėra, parodys 0; po prisijungimo atsinaujins)
            UpdateMoney(GameModel.Instance != null ? GameModel.Instance.Money : 0);
            UpdateGold(GameModel.Instance != null ? GameModel.Instance.Gold : 0);
        }

        private void OnDisable()
        {
            if (waitCo != null) { StopCoroutine(waitCo); waitCo = null; }

            if (subscribed && GameModel.Instance != null)
            {
                GameModel.Instance.OnMoneyChanged -= UpdateMoney;
                GameModel.Instance.OnGoldChanged -= UpdateGold;
                subscribed = false;
            }
        }

        private void TrySubscribeOrWait()
        {
            if (GameModel.Instance != null)
            {
                if (!subscribed)
                {
                    GameModel.Instance.OnMoneyChanged += UpdateMoney;
                    GameModel.Instance.OnGoldChanged += UpdateGold;
                    subscribed = true;
                }
            }
            else
            {
                if (waitCo == null) waitCo = StartCoroutine(WaitForGameModelThenSubscribe());
            }
        }

        private IEnumerator WaitForGameModelThenSubscribe()
        {
            while (GameModel.Instance == null) yield return null;

            GameModel.Instance.OnMoneyChanged += UpdateMoney;
            GameModel.Instance.OnGoldChanged += UpdateGold;
            subscribed = true;
            waitCo = null;

            // po prisijungimo iškart atnaujinam rodmenis
            UpdateMoney(GameModel.Instance.Money);
            UpdateGold(GameModel.Instance.Gold);
        }

        private void UpdateMoney(double money)
        {
            if (moneyValueText)
                moneyValueText.text = "$" + NumberAbbreviations.Format(money);
        }

        private void UpdateGold(double gold)
        {
            if (goldValueText)
                goldValueText.text = "★ " + NumberAbbreviations.Format(gold);
        }
    }
}

