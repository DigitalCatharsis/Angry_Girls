using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace Angry_Girls
{
    /// <summary>
    /// Types of UI screens.
    /// </summary>
    public enum UIScreenType
    {
        MainMenu,
        MissionPreparation,
        Gameplay
    }

    /// <summary>
    /// Types of scenes in the game.
    /// </summary>
    public enum SceneType
    {
        None = 0,
        Bootstrap = 1,
        MainMenuScene = 2,
        MissionPreparation = 3,
        Loading = 4,
        Level_0 = 5,
        Level_1 = 6
    }

    /// <summary>
    /// Manages navigation between UI screens and scenes.
    /// </summary>
    public class NavigationManager
    {
        private static SceneType _savedMission;
        /// <summary>
        /// Navigate to a specific scene.
        /// </summary>
        public static async UniTask NavigateToScene(SceneType sceneType)
        {
            UIManager.Instance.ShowLoadingScreen(true);

            //clear garbage from previous Scene
            DOTween.KillAll();
            CoreManager.Instance.PoolManager.ClearAllPools();


            var asyncOp = SceneManager.LoadSceneAsync(sceneType.ToString());

            while (!asyncOp.isDone)
            {
                await UniTask.Yield();
            }

            await InitializeSceneUI(sceneType);

            UIManager.Instance.ShowLoadingScreen(false);
        }

        private static async UniTask InitializeSceneUI(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.MainMenuScene:
                    UIManager.Instance.ShowScreen<UI_MainMenuScreen>(false);
                    break;
                case SceneType.MissionPreparation:
                    UIManager.Instance.ShowScreen<UI_MissionPreparationScreen>(false);
                    break;
            }
            await UniTask.CompletedTask;
        }

        public static async UniTask NavigateToLastMission()
        {
            if (_savedMission == SceneType.None) { return; }
            await NavigateToScene(_savedMission);
        }

        public static void SetLastMission(SceneType mission)
        {
            _savedMission = mission;
        }
    }
}