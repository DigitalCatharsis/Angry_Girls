using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Displays launchable character portraits with swap functionality.
    /// </summary>
    public class UI_GameplayCharactersPanel : UI_GameplayManagersComponent
    {
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private Transform _buttonsContainer;
        [SerializeField] private Sprite _fallbackPortrait;
        
        private LaunchManager _launchManager;
        private GameplayCharactersManager _charactersManager;
        private IAssetProvider _assetProvider;
        private readonly List<GameObject> _activeButtons = new();

        public override void Initialize()
        {
            base.Initialize();

            _launchManager = GameplayCoreManager.Instance.LaunchManager;
            _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;
            _assetProvider = CoreManager.Instance.AddressableAssetManager;

            _charactersManager.OnLaunchableCharactersChanged += UpdatePortraits;

            UpdatePortraits();
        }

        public void UpdatePortraits()
        {
            ClearActiveButtons();
            
            var launchableCharacters = _charactersManager.GetLaunchableCharacters();
            if (launchableCharacters.Count == 0) return;
            
            for (int i = 0; i < launchableCharacters.Count; i++)
            {                
                var buttonGO = Instantiate(_buttonPrefab, _buttonsContainer);
                var button = buttonGO.GetComponent<Button>();
                var image = buttonGO.GetComponentInChildren<Image>(true);
                
                if (button == null || image == null)
                {
                    Debug.LogError("Button prefab must contain Button and Image components");
                    Destroy(buttonGO);
                    continue;
                }
                
                var character = launchableCharacters[i];
                LoadAndSetPortraitAsync(character, image);
                
                int logicalIndex = i;
                button.onClick.AddListener(() => OnPortraitClicked(logicalIndex));
                
                _activeButtons.Add(buttonGO);
            }
        }

        private void ClearActiveButtons()
        {
            foreach (var buttonGO in _activeButtons)
            {
                if (buttonGO != null)
                {
                    var button = buttonGO.GetComponent<Button>();
                    if (button != null)
                        button.onClick.RemoveAllListeners();
                    Destroy(buttonGO);
                }
            }
            _activeButtons.Clear();
        }

        private async void LoadAndSetPortraitAsync(CControl character, Image image)
        {
            if (this == null || image == null) return;
            
            if (character.CharacterSettings?.portrait == null || 
                string.IsNullOrEmpty(character.CharacterSettings.portrait.AssetGUID))
            {
                SetPortraitImage(image, _fallbackPortrait);
                return;
            }
            
            try
            {
                var sprite = await _assetProvider.LoadSpriteAsync(character.CharacterSettings.portrait);
                if (this != null && image != null)
                    SetPortraitImage(image, sprite);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to load portrait for {character.name}: {ex.Message}");
                if (this != null && image != null)
                    SetPortraitImage(image, _fallbackPortrait);
            }
        }

        private void SetPortraitImage(Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private void OnPortraitClicked(int clickedLogicalIndex)
        {
            if (clickedLogicalIndex == 0) return;
            
            _launchManager.TrySwapCharacterByIndex(clickedLogicalIndex);
        }

        private void OnDestroy()
        {
            ClearActiveButtons();
            
            if (_charactersManager != null)
                _charactersManager.OnLaunchableCharactersChanged -= UpdatePortraits;
        }
    }
}