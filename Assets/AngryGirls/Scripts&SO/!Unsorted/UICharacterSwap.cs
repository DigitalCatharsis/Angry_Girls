using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class LaunchPortraitUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _containerPanel;
        [SerializeField] private GameObject _portraitPrefab;

        private readonly List<GameObject> _spawnedPortraits = new();
        private LaunchManager _launchManager;

        public void InitPortraits(List<CControl> characters)
        {
            _launchManager = GameLoader.Instance.launchManager;
            Clear();

            foreach (var character in characters)
            {
                CreatePortrait(character);
            }
        }

        private void CreatePortrait(CControl control)
        {
            var portraitGO = Instantiate(_portraitPrefab, _containerPanel.transform);
            portraitGO.name = $"Portrait_{control.name}";

            var image = portraitGO.GetComponentInChildren<Image>();
            image.sprite = control.characterSettings.portrait;

            var index = _spawnedPortraits.Count;
            var button = portraitGO.GetComponent<Button>();
            button.onClick.AddListener(() => _launchManager.TrySwapCharacterByIndex(index));

            _spawnedPortraits.Add(portraitGO);
        }

        public void RemovePortraitAtIndex(int index)
        {
            if (index < 0 || index >= _spawnedPortraits.Count) return;

            var go = _spawnedPortraits[index];
            Destroy(go);
            _spawnedPortraits.RemoveAt(index);
        }

        public void UpdatePortraitOrder(List<CControl> updatedList)
        {
            for (int i = 0; i < _spawnedPortraits.Count && i < updatedList.Count; i++)
            {
                var portraitGO = _spawnedPortraits[i];
                var image = portraitGO.GetComponentInChildren<Image>();
                image.sprite = updatedList[i].characterSettings.portrait;

                int index = i;
                var button = portraitGO.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => _launchManager.TrySwapCharacterByIndex(index));
            }
        }

        public void Clear()
        {
            foreach (var obj in _spawnedPortraits)
            {
                Destroy(obj);
            }
            _spawnedPortraits.Clear();
        }

        // Вызов при смене стадии, чтобы пересоздать UI по новым юнитам
        public void RefreshOnStageChange(List<CControl> newCharacters)
        {
            InitPortraits(newCharacters);
        }
    }
}
