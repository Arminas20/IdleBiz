using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleBiz.Core
{
    /// <summary>
    /// Paleidþia globalias nuostatas ir uþkrauna pagrindinæ scenà.
    /// </summary>
    public sealed class ProjectInitializer : MonoBehaviour
    {
        [SerializeField] private string mainSceneName = "Main";
        [SerializeField] private int targetFps = 60;

        private void Awake()
        {
            // 1) Bendros app nuostatos
            Application.targetFrameRate = targetFps; // valdysim baterijos naudojimà ir sklandumà
            QualitySettings.vSyncCount = 0;         // ant mobilaus naudosim targetFrameRate

            // 2) Pirmo paleidimo pasiruoðimas (pvz., sukurti katalogà iðsaugojimams)
            EnsurePersistentDataFolder();

            // 3) Uzkraunam Main scenà
            LoadMainScene();
        }

        private static void EnsurePersistentDataFolder()
        {
            // persistentDataPath yra unikalus per árenginá katalogas app duomenims
            if (!System.IO.Directory.Exists(Application.persistentDataPath))
            {
                System.IO.Directory.CreateDirectory(Application.persistentDataPath);
            }
        }

        private void LoadMainScene()
        {
            if (SceneManager.GetActiveScene().name != mainSceneName)
            {
                SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);
            }
        }
    }
}

