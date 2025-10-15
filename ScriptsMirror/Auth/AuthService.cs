using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

namespace IdleBiz.Auth
{
    /// <summary>
    /// Firebase Auth servisas: inicializuoja Firebase, seka vartotojo b�sen�,
    /// turi SignUp/SignIn/SignOut, display name keitim� ir sesijos validacij�.
    /// </summary>
    public sealed class AuthService : MonoBehaviour
    {
        public static AuthService Instance { get; private set; }

        public event Action<FirebaseUser> OnAuthStateChanged; // null => atsijungta

        public FirebaseApp App { get; private set; }
        public FirebaseAuth Auth { get; private set; }
        public FirebaseUser User => Auth?.CurrentUser;

        [SerializeField] private bool logDebug = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // (jei erzina �sp�jimai) galima nutildyti iki Error:
            // Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Error;
        }

        private void Start() => InitializeFirebase();

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(t =>
            {
                if (t.Result != DependencyStatus.Available)
                {
                    Debug.LogError("[AuthService] Firebase deps not available: " + t.Result);
                    OnAuthStateChanged?.Invoke(null);
                    return;
                }

                App = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;

                Auth.StateChanged += HandleStateChanged;
                HandleStateChanged(this, EventArgs.Empty); // pradin� b�sena

                if (logDebug) Debug.Log("[AuthService] Firebase initialized.");

                // Paleid�iant � patikrinam ar ke�intas useris dar galioja
                _ = ValidateCurrentUserAsync();
            });
        }

        private void OnDestroy()
        {
            if (Auth != null) Auth.StateChanged -= HandleStateChanged;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Gr��us � app� � pasitikrinam b�sen� (jei vartotoj� i�tryn� nuotoliniu b�du)
            if (hasFocus) _ = ValidateCurrentUserAsync();
        }

        private void HandleStateChanged(object sender, EventArgs e)
        {
            if (logDebug)
            {
                var uid = Auth?.CurrentUser != null ? Auth.CurrentUser.UserId : "null";
                Debug.Log("[AuthService] StateChanged. User: " + uid);
            }
            OnAuthStateChanged?.Invoke(Auth?.CurrentUser);
        }

        // ----------- API -----------

        public Task<FirebaseUser> SignUpWithEmail(string email, string password)
        {
            if (Auth == null) throw new Exception("Auth not ready.");
            return Auth.CreateUserWithEmailAndPasswordAsync(email, password)
                .ContinueWithOnMainThread(t =>
                {
                    if (t.IsFaulted || t.IsCanceled) throw t.Exception ?? new Exception("SignUp canceled");
                    var user = t.Result.User; // Task<AuthResult/UserCredential>.User
                    OnAuthStateChanged?.Invoke(Auth.CurrentUser); // prane�am UI
                    return user;
                });
        }

        public Task<FirebaseUser> SignInWithEmail(string email, string password)
        {
            if (Auth == null) throw new Exception("Auth not ready.");
            return Auth.SignInWithEmailAndPasswordAsync(email, password)
                .ContinueWithOnMainThread(t =>
                {
                    if (t.IsFaulted || t.IsCanceled) throw t.Exception ?? new Exception("SignIn canceled");
                    var user = t.Result.User;
                    OnAuthStateChanged?.Invoke(Auth.CurrentUser); // prane�am UI
                    return user;
                });
        }

        public void SignOut()
        {
            if (Auth == null) return;
            Auth.SignOut();
            OnAuthStateChanged?.Invoke(null);
        }

        public Task UpdateDisplayName(string nickname)
        {
            if (Auth == null || Auth.CurrentUser == null) throw new Exception("No user.");
            var profile = new UserProfile { DisplayName = nickname };

            // Po profilio atnaujinimo � priverstinai persikraunam user��, kad DisplayName pasiekt� UI
            return Auth.CurrentUser.UpdateUserProfileAsync(profile)
                .ContinueWithOnMainThread(async t =>
                {
                    if (t.IsFaulted || t.IsCanceled) throw t.Exception ?? new Exception("Update display name failed");

                    try
                    {
                        await Auth.CurrentUser.ReloadAsync(); // u�tikrina atnaujint� DisplayName
                    }
                    catch (Exception) { /* network ar pan. � ignore */ }

                    OnAuthStateChanged?.Invoke(Auth.CurrentUser);
                });
        }

        public Task SendPasswordResetToCurrentEmail()
        {
            if (Auth == null || Auth.CurrentUser == null) throw new Exception("No user.");
            var email = Auth.CurrentUser.Email;
            return Auth.SendPasswordResetEmailAsync(email)
                .ContinueWithOnMainThread(t =>
                {
                    if (t.IsFaulted || t.IsCanceled) throw t.Exception ?? new Exception("Reset email failed");
                    if (logDebug) Debug.Log("[AuthService] Reset email sent to " + email);
                });
        }

        // ----------- VALIDACIJA -----------

        /// <summary>
        /// Patikrina ar CurrentUser dar egzistuoja serveryje. Jei i�trintas � auto SignOut().
        /// </summary>
        public async Task ValidateCurrentUserAsync()
        {
            if (Auth == null || Auth.CurrentUser == null) return;

            try
            {
                await Auth.CurrentUser.ReloadAsync();
                if (logDebug) Debug.Log("[AuthService] User reload OK: " + Auth.CurrentUser.UserId);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[AuthService] Current user invalid/removed. Auto sign-out. " + e.Message);
                SignOut();
            }
        }

        /// <summary> Patogu kviesti i� UI, jei norisi ��ia ir dabar� patikrinti b�sen�. </summary>
        public Task EnsureUserIsValidNow() => ValidateCurrentUserAsync();
    }
}


