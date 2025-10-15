using Firebase.Auth;
using TMPro;
using UnityEngine;

namespace IdleBiz.Auth
{
    /// <summary> Rodo LoginPanel, kol neprisijungta; ájungia GameUI, kai prisijungiama. </summary>
    public sealed class AuthGate : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private GameObject gameUIRoot; // Canvas/GameUIRoot
        [SerializeField] private GameObject loginPanel; // Canvas/LoginPanel
        [SerializeField] private TMP_Text welcomeText; // optional (pvz., "Hi, name")

        private void OnEnable()
        {
            if (AuthService.Instance != null)
                AuthService.Instance.OnAuthStateChanged += OnAuth;
            Apply(AuthService.Instance?.User);
        }

        private void OnDisable()
        {
            if (AuthService.Instance != null)
                AuthService.Instance.OnAuthStateChanged -= OnAuth;
        }

        private void OnAuth(FirebaseUser user) => Apply(user);

        private void Apply(FirebaseUser user)
        {
            bool logged = user != null;
            if (gameUIRoot) gameUIRoot.SetActive(logged);
            if (loginPanel) loginPanel.SetActive(!logged);

            if (welcomeText)
                welcomeText.text = logged ? $"Welcome, {user.Email}" : "";
        }
    }
}

