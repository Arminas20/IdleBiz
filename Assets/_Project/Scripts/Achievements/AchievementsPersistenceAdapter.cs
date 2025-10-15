using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IdleBiz.Achievements
{
    /// Laiko claim'int� pasiekim� ID ir prane�a, kai kas nors k� tik �Claim�.
    public sealed class AchievementsPersistenceAdapter : MonoBehaviour, IAchievementsPersistence
    {
        [SerializeField] private List<string> claimed = new();

        /// TIESIOGIN� sinchronin� prieiga � be joki� Task/async.
        public IReadOnlyList<string> Claimed => claimed;

        /// Prane�imas SaveOrchestratoriui, kad i�kart i�saugot�.
        public event System.Action<string> OnClaimed;

        // --- IAchievementsPersistence (sync API) ---
        public IReadOnlyCollection<string> GetClaimedIds() => claimed;

        public void ApplyClaimedIds(IEnumerable<string> ids)
        {
            claimed = ids != null
                ? ids.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList()
                : new List<string>();
            // jei reikia � �ia gali atnaujinti UI (pvz., eilu�i� �Completed�)
        }

        public void MarkClaimed(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!claimed.Contains(id)) claimed.Add(id);
            OnClaimed?.Invoke(id); // leid�ia SaveOrchestratoriui i�kart i�saugoti
        }
    }
}
