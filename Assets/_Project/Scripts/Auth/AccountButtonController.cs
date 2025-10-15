using IdleBiz.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

namespace IdleBiz.UI
{
    /// <summary>
    /// TopBar mygtukas: rodo "Sign up / Log in" arba prisijungusio nickname.
    /// Atidaro AccountPanel.
    /// </summary>
    public sealed class AccountButtonController : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject panelToOpen; // AccountPanel

        private void Awake()
        {
            if (!button) button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button) button.onClick.AddListener(OpenPanel);
            if (AuthService.Instance != null)
                AuthService.Instance.OnAuthStateChanged += OnAuthChanged;

            Apply(AuthService.Instance?.User);
        }

        private void OnDisable()
        {
            if (button) button.onClick.RemoveListener(OpenPanel);
            if (AuthService.Instance != null)
                AuthService.Instance.OnAuthStateChanged -= OnAuthChanged;
        }

        private void OnAuthChanged(FirebaseUser user) => Apply(user);

        private void Apply(FirebaseUser user)
        {
            if (!label) return;

            if (user == null)
            {
                label.text = "Sign up / Log in";
                return;
            }

            string nick = user.DisplayName;
            if (string.IsNullOrWhiteSpace(nick))
            {
                var email = user.Email;
                nick = (!string.IsNullOrEmpty(email) && email.Contains("@")) ? email.Split('@')[0] : "Account";
            }

            label.text = nick;
        }

        private void OpenPanel()
        {
            if (panelToOpen) panelToOpen.SetActive(true);
        }
    }
}
