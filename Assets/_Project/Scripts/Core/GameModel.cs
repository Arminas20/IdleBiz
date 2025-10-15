using System;
using UnityEngine;

namespace IdleBiz.Core
{
    /// <summary>
    /// Pagrindinis þaidimo modelis: pinigai, lifetime, gold, tap bonusai.
    /// </summary>
    public sealed class GameModel : MonoBehaviour
    {
        public static GameModel Instance { get; private set; }

        public event Action<double> OnMoneyChanged;
        public event Action<double> OnLifetimeChanged;
        public event Action<double> OnGoldChanged;
        public event Action OnUpgradesChanged;

        [SerializeField] private double money;
        public double Money
        {
            get => money;
            private set { money = Math.Max(0, value); OnMoneyChanged?.Invoke(money); }
        }

        [SerializeField] private double lifetimeMoney;
        public double LifetimeMoney
        {
            get => lifetimeMoney;
            private set { lifetimeMoney = Math.Max(0, value); OnLifetimeChanged?.Invoke(lifetimeMoney); }
        }

        [SerializeField] private double gold;
        public double Gold
        {
            get => gold;
            private set { gold = Math.Max(0, value); OnGoldChanged?.Invoke(gold); }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary> Prideda pinigø. Teigiama suma didina ir lifetime. </summary>
        public void AddMoney(double amount)
        {
            if (amount > 0) LifetimeMoney += amount;
            Money += amount;
        }

        public bool TrySpend(double amount)
        {
            if (money < amount) return false;
            Money -= amount;
            return true;
        }

        // --- Gold
        public void AddGold(double amount)
        {
            if (amount <= 0) return;
            Gold += amount;
        }

        public bool TrySpendGold(double amount)
        {
            if (gold < amount) return false;
            Gold -= amount;
            return true;
        }

        // --- TAP bonusai ---
        [SerializeField] private double tapFlatBonus = 0;
        [SerializeField] private double tapMultiplier = 1.0;

        public void AddTapFlat(double amount) { tapFlatBonus += amount; OnUpgradesChanged?.Invoke(); }
        public void MultiplyTap(double factor) { tapMultiplier *= factor; OnUpgradesChanged?.Invoke(); }
        public double ComputeTapReward(double baseTap) => (baseTap + tapFlatBonus) * tapMultiplier;

        // ========= NAUJA: ákëlimo metodas =========
        /// <summary> Uþkrauna reikðmes ið áraðo (nekoreguoja tap bonusø). </summary>
        public void LoadFromSave(double moneyValue, double lifetimeValue, double goldValue)
        {
            money = Math.Max(0, moneyValue);
            lifetimeMoney = Math.Max(0, lifetimeValue);
            gold = Math.Max(0, goldValue);

            OnMoneyChanged?.Invoke(money);
            OnLifetimeChanged?.Invoke(lifetimeMoney);
            OnGoldChanged?.Invoke(gold);
        }
        // Naujiena GameModel'e – kad galëtume perskaièiuoti bonusus atstatant lygius
        public void ResetTapBonuses()
        {
            // nustatom á pradinius
            var needsInvoke = true;
            // jei turi specialiø side-effect'ø – prireikus, gali èia pridëti
            // (èia tiesiog gràþinam prie startiniø reikðmiø)
            System.Reflection.FieldInfo flatF = typeof(GameModel).GetField("tapFlatBonus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo multF = typeof(GameModel).GetField("tapMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (flatF != null) flatF.SetValue(this, 0d);
            if (multF != null) multF.SetValue(this, 1d);
            if (needsInvoke) OnUpgradesChanged?.Invoke();
        }

    }
}


