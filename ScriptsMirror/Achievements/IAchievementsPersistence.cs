using System.Collections.Generic;

namespace IdleBiz.Achievements
{
    /// Minimalus, SINCHRONINIS pasiekim� b�senos kontraktas saugojimui.
    public interface IAchievementsPersistence
    {
        /// Visi jau �claim�inti� pasiekim� ID.
        IReadOnlyCollection<string> GetClaimedIds();

        /// Pritaiko (u�deda) claim�intus pasiekimus i� saugyklos.
        void ApplyClaimedIds(IEnumerable<string> ids);

        /// Pa�ymi vien� ID kaip claim�int�.
        void MarkClaimed(string id);
    }
}

