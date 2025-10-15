using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firebase.Auth;
using IdleBiz.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM || UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IdleBiz.UI
{
    /// <summary>
    /// Account modalas: neprisijungus – Tabs (Sign up / Log in) ir vëliau pasirinktà formà;
    /// prisijungus – AuthGroup (nickname, change password, switch account (neveiksmingas)).
    /// </summary>
    public sealed class AccountPanelController : MonoBehaviour
    {
        [Header("Groups")]
        [SerializeField] private GameObject unauthGroup;  // Tabs + Forms grupë
        [SerializeField] private GameObject authGroup;    // Prisijungusio vartotojo grupë

        [Header("Tabs container (unauth)")]
        [SerializeField] private GameObject tabsRoot;     // UnauthGroup/Tabs
        [SerializeField] private Button signUpTabBtn;     // Tabs/SignUpTabBtn
        [SerializeField] private Button logInTabBtn;      // Tabs/LogInTabBtn

        [Header("Forms (unauth)")]
        [SerializeField] private GameObject signUpForm;   // UnauthGroup/SignUpForm
        [SerializeField] private GameObject logInForm;    // UnauthGroup/LogInForm

        [Header("SignUp form")]
        [SerializeField] private TMP_InputField suEmail;
        [SerializeField] private TMP_InputField suNickname;
        [SerializeField] private TMP_InputField suPassword;
        [SerializeField] private Button suConfirmBtn;

        [Header("LogIn form")]
        [SerializeField] private TMP_InputField liEmail;
        [SerializeField] private TMP_InputField liPassword;
        [SerializeField] private Button liConfirmBtn;

        [Header("Auth actions")]
        [SerializeField] private TMP_Text nickLabel;
        [SerializeField] private Button changePasswordBtn;
        [SerializeField] private Button switchAccountBtn; // NEVEIKSMINGAS (disabled)

        [Header("Common")]
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private Button closeBtn;     // X mygtukas
        [SerializeField] private Button bgCloseBtn;   // tamsus BG mygtukas (optional)
        [SerializeField] private Button backToTabsBtn;// „Back“ virðuje (rodoma tik formose)
        [SerializeField] private GameObject spinner;  // optional

        private void OnEnable()
        {
            if (signUpTabBtn) signUpTabBtn.onClick.AddListener(() => ShowTab(true));
            if (logInTabBtn) logInTabBtn.onClick.AddListener(() => ShowTab(false));
            if (suConfirmBtn) suConfirmBtn.onClick.AddListener(DoSignUp);
            if (liConfirmBtn) liConfirmBtn.onClick.AddListener(DoLogIn);
            if (changePasswordBtn) changePasswordBtn.onClick.AddListener(DoChangePassword);

            // 1) Switch account – NEVEIKSMINGAS:
            if (switchAccountBtn)
            {
                switchAccountBtn.interactable = false; // pilkas mygtukas
                // arba palik aktyvø su þinute:
                // switchAccountBtn.onClick.AddListener(()=> SetError("Switch account – coming soon"));
            }

            if (closeBtn) closeBtn.onClick.AddListener(ClosePanel);
            if (bgCloseBtn) bgCloseBtn.onClick.AddListener(ClosePanel);
            if (backToTabsBtn) backToTabsBtn.onClick.AddListener(ShowTabsOnly);

            if (AuthService.Instance != null)
                AuthService.Instance.OnAuthStateChanged += OnAuthChanged;

            Apply(AuthService.Instance?.User);

            // Kiekvienà kartà atsidarius panelæ – jei neprisijungæs, rodyk tik Tabs
            if (AuthService.Instance?.User == null)
                ShowTabsOnly();
        }

        private void OnDisable()
        {
            if (signUpTabBtn) signUpTabBtn.onClick.RemoveAllListeners();
            if (logInTabBtn) logInTabBtn.onClick.RemoveAllListeners();
            if (suConfirmBtn) suConfirmBtn.onClick.RemoveAllListeners();
            if (liConfirmBtn) liConfirmBtn.onClick.RemoveAllListeners();
            if (changePasswordBtn) changePasswordBtn.onClick.RemoveAllListeners();
            if (switchAccountBtn) switchAccountBtn.onClick.RemoveAllListeners();
            if (closeBtn) closeBtn.onClick.RemoveAllListeners();
            if (bgCloseBtn) bgCloseBtn.onClick.RemoveAllListeners();
            if (backToTabsBtn) backToTabsBtn.onClick.RemoveAllListeners();

            if (AuthService.Instance != null)
                AuthService.Instance.OnAuthStateChanged -= OnAuthChanged;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM || UNITY_INPUT_SYSTEM
            if (gameObject.activeSelf)
            {
                if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                    (Gamepad.current != null && (Gamepad.current.bButton.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)))
                {
                    ClosePanel();
                }
            }
#else
            if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
                ClosePanel();
#endif
        }

        private void OnAuthChanged(FirebaseUser user) => Apply(user);

        private void Apply(FirebaseUser user)
        {
            bool logged = user != null;

            if (unauthGroup) unauthGroup.SetActive(!logged);
            if (authGroup) authGroup.SetActive(logged);

            if (!logged)
            {
                SetBusy(false);
                SetError("");
                ShowTabsOnly(); // svarbu: tik Tabs, be formø
            }
            else
            {
                var nick = string.IsNullOrEmpty(user.DisplayName)
                    ? (string.IsNullOrEmpty(user.Email) ? "Account" : user.Email.Split('@')[0])
                    : user.DisplayName;
                if (nickLabel) nickLabel.text = $"Logged in as: {nick}";
                SetBusy(false);
                SetError("");
            }
        }

        // ---------- Rodymo reþimai ----------

        /// <summary> Rodo tik tab mygtukus, slepia abi formas. </summary>
        private void ShowTabsOnly()
        {
            if (tabsRoot) tabsRoot.SetActive(true);
            if (signUpForm) signUpForm.SetActive(false);
            if (logInForm) logInForm.SetActive(false);

            if (backToTabsBtn) backToTabsBtn.gameObject.SetActive(false); // tabs reþime „Back“ nereik
            if (signUpTabBtn) signUpTabBtn.interactable = true;
            if (logInTabBtn) logInTabBtn.interactable = true;

            // iðvalom laukus (nebûtina)
            if (suEmail) suEmail.text = "";
            if (suNickname) suNickname.text = "";
            if (suPassword) suPassword.text = "";
            if (liEmail) liEmail.text = "";
            if (liPassword) liPassword.text = "";
        }

        /// <summary> Pasirinktas tab: slepiam Tabs, rodome tik vienà formà. </summary>
        private void ShowTab(bool signUp)
        {
            if (tabsRoot) tabsRoot.SetActive(false);     // <- esminis pakeitimas
            if (signUpForm) signUpForm.SetActive(signUp);
            if (logInForm) logInForm.SetActive(!signUp);

            if (backToTabsBtn) backToTabsBtn.gameObject.SetActive(true); // atsiranda „Back“
        }

        // ---------- Utilitai ----------

        private void SetBusy(bool on)
        {
            if (spinner) spinner.SetActive(on);
            if (suConfirmBtn) suConfirmBtn.interactable = !on;
            if (liConfirmBtn) liConfirmBtn.interactable = !on;
        }

        private void SetError(string msg) { if (errorText) errorText.text = msg; }

        private bool ValidateEmail(string s) => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^\S+@\S+\.\S+$");
        private bool ValidatePassword(string s) => !string.IsNullOrEmpty(s) && s.Length >= 6;

        public void ClosePanel() => gameObject.SetActive(false);

        // ---------- Actions ----------

        private async void DoSignUp()
        {
            var email = suEmail ? suEmail.text.Trim() : "";
            var nickname = suNickname ? suNickname.text.Trim() : "";
            var pass = suPassword ? suPassword.text : "";

            if (!ValidateEmail(email)) { SetError("Enter a valid email."); return; }
            if (string.IsNullOrEmpty(nickname)) { SetError("Enter a nickname."); return; }
            if (!ValidatePassword(pass)) { SetError("Password must be at least 6 chars."); return; }

            await Run(async () =>
            {
                var user = await AuthService.Instance.SignUpWithEmail(email, pass);
                await AuthService.Instance.UpdateDisplayName(nickname);
                // TODO: èia bus guest->account migracija
                gameObject.SetActive(false);
            });
        }

        private async void DoLogIn()
        {
            var email = liEmail ? liEmail.text.Trim() : "";
            var pass = liPassword ? liPassword.text : "";

            if (!ValidateEmail(email)) { SetError("Enter a valid email."); return; }
            if (!ValidatePassword(pass)) { SetError("Password must be at least 6 chars."); return; }

            await Run(async () =>
            {
                var user = await AuthService.Instance.SignInWithEmail(email, pass);
                // TODO: èia bus account load ið debesies
                gameObject.SetActive(false);
            });
        }

        private async void DoChangePassword()
        {
            await Run(async () =>
            {
                await AuthService.Instance.SendPasswordResetToCurrentEmail();
                SetError("Password reset email sent.");
            });
        }

        // Kol kas neveiksmingas (NO-OP). Paliktas tik jei kaþkur prisikabino listener'is.
        private void DoSwitchAccount()
        {
            // NO-OP
        }

        private async Task Run(Func<Task> op)
        {
            try
            {
                SetError("");
                SetBusy(true);
                await op();
            }
            catch (Exception e)
            {
                var msg = e.InnerException != null ? e.InnerException.Message : e.Message;
                SetError(msg);
            }
            finally
            {
                SetBusy(false);
            }
        }
    }
}




