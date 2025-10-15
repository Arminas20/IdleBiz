using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

namespace IdleBiz.Saves
{
    /// Saugo Firestore: users/{uid}
    public sealed class CloudSaveProvider : ISaveProvider
    {
        private readonly FirebaseFirestore _db;
        private readonly FirebaseUser _user;

        public CloudSaveProvider(FirebaseFirestore db, FirebaseUser user)
        {
            _db = db;
            _user = user;
        }

        private DocumentReference Doc => _db.Collection("users").Document(_user.UserId);

        public Task<SaveData> LoadAsync()
        {
            return Doc.GetSnapshotAsync().ContinueWithOnMainThread(t =>
            {
                var snap = t.Result;
                if (!snap.Exists) return null;

                var d = snap.ToDictionary();

                var data = new SaveData
                {
                    money = d.TryGetValue("money", out var m) ? System.Convert.ToDouble(m) : 0,
                    lifetime = d.TryGetValue("lifetime", out var lt) ? System.Convert.ToDouble(lt) : 0,
                    gold = d.TryGetValue("gold", out var g) ? System.Convert.ToInt32(g) : 0,
                    currentBusinessId = d.TryGetValue("currentBusinessId", out var cb) ? System.Convert.ToInt32(cb) : 0,
                    updatedAtUnix = d.TryGetValue("updatedAtUnix", out var up) ? System.Convert.ToInt64(up) : 0,
                    // >>> new: upgrades
                    upgradeLvls = d.TryGetValue("upgradeLvls", out var ul) && ul is IEnumerable<object> ulArr
                        ? ulArr.Select(x => System.Convert.ToInt32(x)).ToArray()
                        : null,
                    // >>> new: claimed achievements
                    claimedAchievements = d.TryGetValue("claimedAchievements", out var ca) && ca is IEnumerable<object> caArr
                        ? caArr.Select(x => x?.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToArray()
                        : null
                };
                return data;
            });
        }

        public Task SaveAsync(SaveData data)
        {
            var dict = new Dictionary<string, object>
            {
                { "money", data.money },
                { "lifetime", data.lifetime },
                { "gold", data.gold },
                { "currentBusinessId", data.currentBusinessId },
                { "updatedAtUnix", data.updatedAtUnix },
                // >>> new: upgrades + claimed
                { "upgradeLvls", data.upgradeLvls ?? System.Array.Empty<int>() },
                { "claimedAchievements", data.claimedAchievements ?? System.Array.Empty<string>() }
            };

            return Doc.SetAsync(dict, SetOptions.MergeAll);
        }
    }
}
