using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// DTO for money serialization.
    /// </summary>
    [Serializable]
    public class CreditsSaveData
    {
        public int credits;
    }

    /// <summary>
    /// Data class for money serialization.
    /// </summary>
    [Serializable]
    public class CreditsData : ISaveData<CreditsData, CreditsSaveData>
    {
        public CreditsData() { _credits = default; }
        public CreditsData(int credits)
        {
            this._credits = credits;
        }

        private int _credits;

        public int Credits => _credits;

        public UniTask UpdateFromSaveAsync(CreditsSaveData saveData)
        {
            if (saveData == null)
            {
                throw new Exception("CreditsData save is null");
            }

            this._credits = saveData.credits;
            return UniTask.CompletedTask;
        }

        public CreditsSaveData ConvertToSaveData()
        {
            return new CreditsSaveData { credits = _credits };
        }

        public void ResetData()
        {
            this._credits = 0;
        }

        /// <summary>
        /// Change the current money amount by the specified value.
        /// </summary>
        public void SetCredits(int amount)
        {
            _credits += amount;
            _credits = Mathf.Max(0, _credits);
        }
    }

    /// <summary>
    /// Manages player's money storage and events.
    /// </summary>
    public sealed class CreditsManager: ISaveReinitManager<DefaultSaveTemplate, CreditsSaveData, CreditsData>
    {
        private CreditsData _creditsData = new();
        public event Action OnDataChanged;
        public CreditsData CreditsData => _creditsData;

        public CreditsSaveData ConvertDataForSave()
        {
            return _creditsData.ConvertToSaveData();
        }

        public void ResetManagersData()
        {
            _creditsData.ResetData();
            OnDataChanged?.Invoke();
        }

        public UniTask ReinitDataFromTemplateAsync(DefaultSaveTemplate template)
        {
            _creditsData.SetCredits(template.startingCredits);
            OnDataChanged?.Invoke();
            return UniTask.CompletedTask;
        }

        public async UniTask ReinitDataFromSaveAsync(CreditsSaveData saveData)
        {
            await _creditsData.UpdateFromSaveAsync(saveData);
        }


        /// <summary>
        /// Set the current money amount.
        /// </summary>
        public void SetCredits(int credits)
        {
            _creditsData.SetCredits(credits);
            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// Get the current money amount.
        /// </summary>
        public int GetCredits()
        {
            return _creditsData.Credits;
        }
    }
}