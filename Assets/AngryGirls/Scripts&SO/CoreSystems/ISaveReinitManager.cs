using Cysharp.Threading.Tasks;
using System;

namespace Angry_Girls
{
    /// <summary>
    /// A manager that manages data, stores it, and can be reinitialized
    /// </summary>
    public interface ISaveReinitManager<TTemplate, TSaveData, TSaveDataOutput>
        where TTemplate : class
        where TSaveData : class
    {
        public event Action OnDataChanged;

        /// <summary>
        /// Completely clears all manager data
        /// </summary>
        public void ResetManagersData();

        /// <summary>
        /// Initializes the manager for a new game from a template
        /// </summary>
        public UniTask ReinitDataFromTemplateAsync(TTemplate template);

        /// <summary>
        /// Initializes the manager from save data
        /// </summary>
        public UniTask ReinitDataFromSaveAsync(TSaveData saveData);

        /// <summary>
        /// Gets the current data for saving
        /// </summary>
        public TSaveData ConvertDataForSave();
    }
}