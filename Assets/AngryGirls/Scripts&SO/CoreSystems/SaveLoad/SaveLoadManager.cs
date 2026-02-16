using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Unified manager for saving/loading game data via ISaveReinitManager interface.
    /// </summary>
    public class SaveLoadManager
    {
        private readonly List<Action> _saveActions = new();
        private readonly List<Func<UniTask>> _loadActions = new();

        /// <summary>
        /// Register manager with its save/load delegates.
        /// </summary>
        public void RegisterManager<TManager, TSaveData>(TManager manager,
            Func<TManager, TSaveData> saveDelegate,
            Func<TManager, TSaveData, UniTask> loadDelegate)
            where TManager : class
            where TSaveData : class
        {
            if (manager == null)
            {
                Debug.LogWarning($"{nameof(SaveLoadManager)}: Null manager for {typeof(TManager).Name}");
                return;
            }

            _saveActions.Add(() =>
            {
                try
                {
                    var data = saveDelegate(manager); 
                    if (data != null)
                        Repository.SetData(data); 
                }
                catch (Exception ex)
                {
                    Debug.LogError($"SaveLoadManager: Save error for {typeof(TManager).Name}: {ex.Message}");
                }
            });

            _loadActions.Add(async () =>
            {
                //try
                //{
                    var data = Repository.GetData<TSaveData>();
                    if (data != null)
                        await loadDelegate(manager, data);
                    else
                        Debug.Log($"SaveLoadManager:No save data for {typeof(TManager).Name} (key: {typeof(TSaveData).Name})");
                //}
                //catch (Exception ex)
                //{
                //    Debug.LogError($"SaveLoadManager: Load error for {typeof(TManager).Name}: {ex.Message}");
                //}
            });

            Debug.Log($"SaveLoadManager: Registered {typeof(TManager).Name} with save type {typeof(TSaveData).Name}");
        }

        public void SaveGame()
        {
            Debug.Log("SaveLoadManager: Starting save...");

            foreach (var action in _saveActions)
                action();

            Repository.SaveState(); 
            Debug.Log("SaveLoadManager: Save completed");
        }

        public async UniTask LoadGameAsync()
        {
            Debug.Log("SaveLoadManager: Starting load...");

            Repository.LoadState();

            var tasks = new List<UniTask>();
            foreach (var action in _loadActions)
                tasks.Add(action());

            await UniTask.WhenAll(tasks);
            Debug.Log("SaveLoadManager: Load completed");
        }
    }
}