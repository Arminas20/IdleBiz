using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleBiz.Core
{
    /// <summary>
    /// Paleid�ia globalias nuostatas ir u�krauna pagrindin� scen�.
    /// </summary>
    public sealed class ProjectInitializer : MonoBehaviour
    {
        [SerializeField] private string mainSceneName = "Main";
        [SerializeField] private int targetFps = 60;

        private void Awake()
        {
            // 1) Bendros app nuostatos
            Application.targetFrameRate = targetFps; // valdysim baterijos naudojim� ir sklandum�
            QualitySettings.vSyncCount = 0;         // ant mobilaus naudosim targetFrameRate

            // 2) Pirmo paleidimo pasiruo�imas (pvz., sukurti katalog� i�saugojimams)
            EnsurePersistentDataFolder();

            // 3) Uzkraunam Main scen�
            LoadMainScene();
        }

        private static void EnsurePersistentDataFolder()
        {
            // persistentDataPath yra unikalus per �rengin� katalogas app duomenims
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

