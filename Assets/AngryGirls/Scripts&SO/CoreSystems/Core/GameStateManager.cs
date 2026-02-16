using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages game state including progress and player data
    /// </summary>
    public class GameStateManager
    {
        public static GameStateManager Instance { get; private set; }
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
        /// Start a new game
        /// </summary>
        public async UniTask NewGame()
        {
            try
            {
                Debug.Log("GameStateManager: Starting New Game with default template...");

                // 1. Get template from CoreManager
                var template = CoreManager.Instance.DefaultSaveTemplate;
                if (template == null)
                {
                    throw new Exception("GameStateManager: DefaultSaveTemplate is not set in CoreManager!");
                }

                // 2. Initialize fields (premission managers mostly)

                var charactersManager = CoreManager.Instance.CharactersManager;
                var missionManager = CoreManager.Instance.MissionsManager;
                var creditsManager = CoreManager.Instance.CreditsManager;
                var inventoryManager = CoreManager.Instance.InventoryManager;
                var shopManager = CoreManager.Instance.ShopManager;

                //    charactersManager
                //    missionManager
                //    moneyStorage
                //    inventoryManager
                //    shopManager

                // 3. Reset all managers but Settings
                ResetManagers(charactersManager, missionManager, creditsManager, inventoryManager, shopManager);

                // 4. Reinit from defaultTemplate
                var unitasks = new List<UniTask>();
                unitasks.Add(charactersManager.ReinitDataFromTemplateAsync(template));
                unitasks.Add(missionManager.ReinitDataFromTemplateAsync(template));
                unitasks.Add(creditsManager.ReinitDataFromTemplateAsync(template));
                unitasks.Add(inventoryManager.ReinitDataFromTemplateAsync(template));
                unitasks.Add(shopManager.ReinitDataFromTemplateAsync(template));

                await UniTask.WhenAll(unitasks);

                // 5. Save BEFORE entering the scene
                CoreManager.Instance.SaveLoadManager.SaveGame();

                // 6. Go to scene
                await NavigationManager.NavigateToScene(SceneType.MissionPreparation);

                Debug.Log("GameStateManager: New game initialized and saved successfully");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void ResetManagers(CharactersManager charactersManager, MissionsManager missionManager, CreditsManager creditsManager, InventoryManager inventoryManager, ShopManager shopManager)
        {
            charactersManager.ResetManagersData();
            missionManager.ResetManagersData();
            creditsManager.ResetManagersData();
            inventoryManager.ResetManagersData();
            shopManager.ResetManagersData();
        }

        /// <summary>
        /// Continue existing game
        /// </summary>
        public async UniTask ContinueGame()
        {
            var charactersManager = CoreManager.Instance.CharactersManager;
            var missionManager = CoreManager.Instance.MissionsManager;
            var creditsManager = CoreManager.Instance.CreditsManager;
            var inventoryManager = CoreManager.Instance.InventoryManager;
            var shopManager = CoreManager.Instance.ShopManager;

            ResetManagers(charactersManager, missionManager, creditsManager, inventoryManager, shopManager);

            DOTween.KillAll();
            CoreManager.Instance.PoolManager.ClearAllPools();

            Debug.Log("GameStateManager: Continuing Game...");
            await CoreManager.Instance.SaveLoadManager.LoadGameAsync();
            await NavigationManager.NavigateToScene(SceneType.MissionPreparation);
        }
    }
}