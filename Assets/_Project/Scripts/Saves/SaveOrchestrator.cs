using System.Threading.Tasks;
using Firebase.Auth;
#if FIREBASE_FIRESTORE
using Firebase.Firestore;
#endif
using IdleBiz.Auth;
using IdleBiz.Core;
using IdleBiz.UI;
using IdleBiz.Achievements;
using UnityEngine;

namespace IdleBiz.Saves
{
    /// Orkestruoja saugojimà:
    /// - sveèiui: LocalSaveProvider;
    /// - prisijungus: Cloud (jei FIREBASE_FIRESTORE) + lokali kopija;
    /// - saugo po autosave, application pause/quit, po upgrade pirkimo ir po Achievement „Claim“.
    public sealed class SaveOrchestrator : MonoBehaviour
    {
        public static SaveOrchestrator Instance { get; private set; }

        [Header("Refs")]
        [SerializeField] private GameModel gameModel;
        [SerializeField] private UpgradesSystem upgradesSystem;                       // pririði Inspector’iuje arba registruosis OnEnable
        [SerializeField] private AchievementsPersistenceAdapter achievementsProvider; // KONKRETUS tipas (ne MonoBehaviour)

        [Header("Auto-save")]
        [SerializeField] private float autosaveIntervalSec = 20f;

        private ISaveProvider _provider;
        private LocalSaveProvider _guestLocal;
        private LocalSaveProvider _userLocal; // bus priskirta tik su FIREBASE_FIRESTORE
#if FIREBASE_FIRESTORE
        private FirebaseFirestore _firestore;
#endif
        private float _timer;

        // Kol sistemos dar neuþsikrovusios – laikom pending duomenis
        private int[] _pendingUpgradeLvls;
        private string[] _pendingClaimed;

        private IAchievementsPersistence Ach => achievementsProvider; // adapteris ágyvendina sàsajà

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (AuthService.Instance != null)
                AuthService.Instance.OnAuthStateChanged += OnAuthChanged;
        }

        private void OnDisable()
        {
            if (AuthService.Instance != null)
                AuthService.Instance.OnAuthStateChanged -= OnAuthChanged;

            if (upgradesSystem != null)
                upgradesSystem.OnAnyUpgradePurchased -= HandleUpgradePurchased;

            if (achievementsProvider != null)
                achievementsProvider.OnClaimed -= HandleAchievementClaimed;
        }

        private async void Start()
        {
            _guestLocal = new LocalSaveProvider("save_guest");
            await SwitchProvider(AuthService.Instance?.User);

            // Jei laukai Inspector’iuje uþpildyti – prisikabinam prie event’ø ir pritaikom pending
            if (upgradesSystem != null) RegisterUpgradesSystem(upgradesSystem);
            if (achievementsProvider != null) RegisterAchievementsProvider(achievementsProvider);
        }

        private void Update()
        {
            if (_provider == null) return;
            _timer += Time.unscaledDeltaTime;
            if (_timer >= autosaveIntervalSec)
            {
                _timer = 0;
                _ = SaveNow();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) _ = SaveNow();
        }

        private void OnApplicationQuit()
        {
            _ = SaveNow();
        }

        private async void OnAuthChanged(FirebaseUser user) => await SwitchProvider(user);

        private async Task SwitchProvider(FirebaseUser user)
        {
            if (user == null)
            {
                _provider = _guestLocal;
                var data = await _provider.LoadAsync();
                if (data == null) data = CollectFromGame();
                await ApplyToGame(data);
                return;
            }

#if FIREBASE_FIRESTORE
            _firestore = FirebaseFirestore.DefaultInstance;
            _provider = new CloudSaveProvider(_firestore, user);
            _userLocal = new LocalSaveProvider($"save_{user.UserId}");

            var cloud = await _provider.LoadAsync();
            if (cloud != null)
            {
                await ApplyToGame(cloud);
                await _userLocal.SaveAsync(cloud);
            }
            else
            {
                var guest = await _guestLocal.LoadAsync();
                var data = guest ?? CollectFromGame();
                data.updatedAtUnix = Now();
                await _provider.SaveAsync(data);
                await _userLocal.SaveAsync(data);
            }
#else
            Debug.LogWarning("[SaveOrchestrator] FIREBASE_FIRESTORE not defined. Using LOCAL saves.");
            _provider = _guestLocal;
            var local = await _provider.LoadAsync();
            if (local == null) local = CollectFromGame();
            await ApplyToGame(local);
#endif
        }

        public async Task SaveNow()
        {
            if (_provider == null || gameModel == null) return;

            var data = CollectFromGame();
            data.updatedAtUnix = Now();
            await _provider.SaveAsync(data);

#if FIREBASE_FIRESTORE
            if (_userLocal != null) await _userLocal.SaveAsync(data);
#else
            if (_provider == _guestLocal) await _guestLocal.SaveAsync(data);
#endif
        }

        // ===== Registracijos ið sistemø (kai jos aktyvuojasi) =====
        public void RegisterUpgradesSystem(UpgradesSystem sys)
        {
            if (upgradesSystem != sys)
            {
                if (upgradesSystem != null)
                    upgradesSystem.OnAnyUpgradePurchased -= HandleUpgradePurchased;
                upgradesSystem = sys;
            }

            upgradesSystem.OnAnyUpgradePurchased -= HandleUpgradePurchased;
            upgradesSystem.OnAnyUpgradePurchased += HandleUpgradePurchased;

            if (_pendingUpgradeLvls != null)
            {
                upgradesSystem.ApplyLevels(_pendingUpgradeLvls);
                _pendingUpgradeLvls = null;
            }
        }

        public void RegisterAchievementsProvider(AchievementsPersistenceAdapter provider)
        {
            if (achievementsProvider != provider)
            {
                if (achievementsProvider != null)
                    achievementsProvider.OnClaimed -= HandleAchievementClaimed;

                achievementsProvider = provider;
            }

            achievementsProvider.OnClaimed -= HandleAchievementClaimed;
            achievementsProvider.OnClaimed += HandleAchievementClaimed;

            // jei turëjome pending – pritaikome dabar
            if (Ach != null && _pendingClaimed != null)
            {
                Ach.ApplyClaimedIds(_pendingClaimed);
                _pendingClaimed = null;
            }
        }

        private void HandleUpgradePurchased() => _ = SaveNow();

        // FIX: parametras neturi bûti "_" (nes kompiliatorius bandë "Task" priskirti stringui)
        private void HandleAchievementClaimed(string id)
        {
            _ = SaveNow();
        }

        // ===== Game <-> SaveData =====
        private SaveData CollectFromGame()
        {
            // — jokio async, jokio LINQ, jokio GetClaimedIds() – imame tiesiai ið adapterio property —
            string[] claimedArray = null;
            if (achievementsProvider != null && achievementsProvider.Claimed != null)
            {
                var list = achievementsProvider.Claimed;
                if (list.Count > 0)
                {
                    claimedArray = new string[list.Count];
                    for (int i = 0; i < list.Count; i++)
                        claimedArray[i] = list[i];
                }
            }

            return new SaveData
            {
                money = gameModel ? gameModel.Money : 0,
                lifetime = gameModel ? gameModel.LifetimeMoney : 0,
                gold = gameModel ? (int)gameModel.Gold : 0,
                currentBusinessId = 0,
                updatedAtUnix = Now(),
                upgradeLvls = upgradesSystem ? upgradesSystem.CollectLevels() : _pendingUpgradeLvls,
                claimedAchievements = (claimedArray != null && claimedArray.Length > 0)
                    ? claimedArray
                    : _pendingClaimed
            };
        }

        private Task ApplyToGame(SaveData d)
        {
            if (gameModel)
                gameModel.LoadFromSave(d.money, d.lifetime, d.gold);

            if (upgradesSystem != null && d.upgradeLvls != null)
                upgradesSystem.ApplyLevels(d.upgradeLvls);
            else
                _pendingUpgradeLvls = d.upgradeLvls;

            if (Ach != null && d.claimedAchievements != null)
                Ach.ApplyClaimedIds(d.claimedAchievements);
            else
                _pendingClaimed = d.claimedAchievements;

            return Task.CompletedTask;
        }

        private static long Now() => System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
