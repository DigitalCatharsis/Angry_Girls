using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Initializes core systems and loads the main menu scene
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        private async void Start()
        {
            Debug.Log("Bootstrap: Starting initialization...");

            if (CoreManager.Instance == null)
            {
                Debug.LogError("Bootstrap: CoreManager not found in the scene!");
                return;
            }

            if (GameStateManager.Instance == null)
            {
                new GameStateManager();
                Debug.Log("Bootstrap: GameStateManager created.");
            }

            await CoreManager.Instance.InitializeSystemsAsync();
            Debug.Log("Bootstrap: Core systems fully initialized.");

            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenuScene");
        }
    }
}