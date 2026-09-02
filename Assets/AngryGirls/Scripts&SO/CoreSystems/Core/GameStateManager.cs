using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages game state including progress and player data.
    /// </summary>
    public class GameStateManager
    {
        public static GameStateManager Instance { get; private set; }

        /// <summary>
        /// Indicates that the current runtime session was started through New Game.
        /// </summary>
        public bool IsNewGameSession { get; private set; }

        public GameStateManager()
        {
            if (Instance != null)
            {
                Debug.LogWarning("GameStateManager already exists!");
                return;
            }

            Instance = this;
        }



        /// <summary>
        /// Marks the current new-game tutorial as consumed.
        /// </summary>
        public void ConsumeNewGameTutorial()
        {
            IsNewGameSession = false;
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        public async UniTask NewGame()
        {
            try
            {
                IsNewGameSession = true;

                Debug.Log(
                    "GameStateManager: Starting New Game with default template...");

                var template =
                    CoreManager.Instance.DefaultSaveTemplate;

                if (template == null)
                {
                    throw new Exception(
                        "GameStateManager: DefaultSaveTemplate is not set in CoreManager!");
                }

                var charactersManager =
                    CoreManager.Instance.CharactersManager;

                var missionManager =
                    CoreManager.Instance.MissionsManager;

                var creditsManager =
                    CoreManager.Instance.CreditsManager;

                var inventoryManager =
                    CoreManager.Instance.InventoryManager;

                var shopManager =
                    CoreManager.Instance.ShopManager;

                ResetManagers(
                    charactersManager,
                    missionManager,
                    creditsManager,
                    inventoryManager,
                    shopManager);

                var unitasks =
                    new List<UniTask>
                    {
                        charactersManager
                            .ReinitDataFromTemplateAsync(
                                template),

                        missionManager
                            .ReinitDataFromTemplateAsync(
                                template),

                        creditsManager
                            .ReinitDataFromTemplateAsync(
                                template),

                        inventoryManager
                            .ReinitDataFromTemplateAsync(
                                template),

                        shopManager
                            .ReinitDataFromTemplateAsync(
                                template)
                    };

                await UniTask.WhenAll(unitasks);

                CoreManager.Instance
                    .SaveLoadManager
                    .SaveGame();

                await NavigationManager.NavigateToScene(
                    SceneType.MissionPreparation);

                Debug.Log(
                    "GameStateManager: New game initialized and saved successfully");
            }
            catch (Exception exception)
            {
                IsNewGameSession = false;
                Debug.LogException(exception);
            }
        }

        private void ResetManagers(
            CharactersManager charactersManager,
            MissionsManager missionManager,
            CreditsManager creditsManager,
            InventoryManager inventoryManager,
            ShopManager shopManager)
        {
            charactersManager.ResetManagersData();
            missionManager.ResetManagersData();
            creditsManager.ResetManagersData();
            inventoryManager.ResetManagersData();
            shopManager.ResetManagersData();
        }

        /// <summary>
        /// Saves the game and returns to mission preparation.
        /// </summary>
        public async UniTask ReturnToMissionPreparation()
        {
            CoreManager.Instance
                .SaveLoadManager
                .SaveGame();

            await NavigationManager.NavigateToScene(
                SceneType.MissionPreparation);
        }

        /// <summary>
        /// Continues an existing saved game.
        /// </summary>
        public async UniTask ContinueGame()
        {
            IsNewGameSession = false;

            var charactersManager =
                CoreManager.Instance.CharactersManager;

            var missionManager =
                CoreManager.Instance.MissionsManager;

            var creditsManager =
                CoreManager.Instance.CreditsManager;

            var inventoryManager =
                CoreManager.Instance.InventoryManager;

            var shopManager =
                CoreManager.Instance.ShopManager;

            ResetManagers(
                charactersManager,
                missionManager,
                creditsManager,
                inventoryManager,
                shopManager);

            DOTween.KillAll();
            CoreManager.Instance
                .PoolManager
                .ClearAllPools();

            Debug.Log(
                "GameStateManager: Continuing Game...");

            await CoreManager.Instance
                .SaveLoadManager
                .LoadGameAsync();

            await NavigationManager.NavigateToScene(
                SceneType.MissionPreparation);
        }
    }
}