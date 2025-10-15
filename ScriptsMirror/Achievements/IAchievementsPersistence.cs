using System.Collections.Generic;

namespace IdleBiz.Achievements
{
    /// Minimalus, SINCHRONINIS pasiekimø bûsenos kontraktas saugojimui.
    public interface IAchievementsPersistence
    {
        /// Visi jau „claim’inti“ pasiekimø ID.
        IReadOnlyCollection<string> GetClaimedIds();

        /// Pritaiko (uþdeda) claim’intus pasiekimus ið saugyklos.
        void ApplyClaimedIds(IEnumerable<string> ids);

        /// Paþymi vienà ID kaip claim’intà.
        void MarkClaimed(string id);
    }
}

