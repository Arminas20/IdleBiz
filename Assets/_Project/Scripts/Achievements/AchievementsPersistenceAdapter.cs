using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IdleBiz.Achievements
{
    /// Laiko claim'intø pasiekimø ID ir praneða, kai kas nors kà tik „Claim“.
    public sealed class AchievementsPersistenceAdapter : MonoBehaviour, IAchievementsPersistence
    {
        [SerializeField] private List<string> claimed = new();

        /// TIESIOGINË sinchroninë prieiga – be jokiø Task/async.
        public IReadOnlyList<string> Claimed => claimed;

        /// Praneðimas SaveOrchestratoriui, kad iðkart iðsaugotø.
        public event System.Action<string> OnClaimed;

        // --- IAchievementsPersistence (sync API) ---
        public IReadOnlyCollection<string> GetClaimedIds() => claimed;

        public void ApplyClaimedIds(IEnumerable<string> ids)
        {
            claimed = ids != null
                ? ids.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList()
                : new List<string>();
            // jei reikia – èia gali atnaujinti UI (pvz., eiluèiø „Completed“)
        }

        public void MarkClaimed(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!claimed.Contains(id)) claimed.Add(id);
            OnClaimed?.Invoke(id); // leidþia SaveOrchestratoriui iðkart iðsaugoti
        }
    }
}
