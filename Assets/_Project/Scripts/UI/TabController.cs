using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization; // dël FormerlySerializedAs

namespace IdleBiz.UI
{
    public sealed class TabController : MonoBehaviour
    {
        [Header("Buttons (bottom bar)")]
        [SerializeField] private Button upgradesBtn;
        [SerializeField] private Button businessBtn;

        [FormerlySerializedAs("rankingBtn")]
        [SerializeField] private Button achievementsBtn;   // naujas vardas (iðlaiko senà assignment)

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject upgradesPanel;
        [SerializeField] private GameObject businessPanel;

        [FormerlySerializedAs("rankingPanel")]
        [SerializeField] private GameObject achievementsPanel; // naujas vardas (iðlaiko senà assignment)

        private void Awake() => ShowMain();

        private void OnEnable()
        {
            if (upgradesBtn) upgradesBtn.onClick.AddListener(ShowUpgrades);
            if (businessBtn) businessBtn.onClick.AddListener(ShowBusiness);
            if (achievementsBtn) achievementsBtn.onClick.AddListener(ShowAchievements);
        }

        private void OnDisable()
        {
            if (upgradesBtn) upgradesBtn.onClick.RemoveListener(ShowUpgrades);
            if (businessBtn) businessBtn.onClick.RemoveListener(ShowBusiness);
            if (achievementsBtn) achievementsBtn.onClick.RemoveListener(ShowAchievements);
        }

        public void ShowMain() => Show(mainPanel);
        public void ShowUpgrades() => Show(upgradesPanel);
        public void ShowBusiness() => Show(businessPanel);
        public void ShowAchievements() => Show(achievementsPanel);

        // Galime palikti suderinamumui:
        public void ShowRanking() => ShowAchievements();

        private void Show(GameObject target)
        {
            if (mainPanel) mainPanel.SetActive(false);
            if (upgradesPanel) upgradesPanel.SetActive(false);
            if (businessPanel) businessPanel.SetActive(false);
            if (achievementsPanel) achievementsPanel.SetActive(false);
            if (target) target.SetActive(true);
        }
    }
}


