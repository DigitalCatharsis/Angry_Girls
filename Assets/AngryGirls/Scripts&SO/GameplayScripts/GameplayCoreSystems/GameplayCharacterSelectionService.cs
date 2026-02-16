//using System.Collections.Generic;

//namespace Angry_Girls
//{
//    /// <summary>
//    /// Service for character selection logic during launch phase.
//    /// Manages selection index and swapping within a provided character list.
//    /// Does NOT store global character state — works only with externally provided list.
//    /// </summary>
//    public class GameplayCharacterSelectionService : GameplayManagerClass
//    {
//        private GameplayCharactersManager _charactersManagers;
//        private int _selectedIndex = 0;

//        public int SelectedIndex { get { return _selectedIndex; } }
//        public List<CControl> _availableCharacters { get { return _charactersManagers.GetLaunchableCharacters(); } }

//        public CControl SelectedCharacter => _availableCharacters.Count > 0 ? _availableCharacters[_selectedIndex] : null;

//        public override void Initialize()
//        {
//            isInitialized = true;
//            GameplayCoreManager.Instance.OnInitialized += LateInitialize;
//        }

//        private void LateInitialize()
//        {
//            GameplayCoreManager.Instance.OnInitialized -= LateInitialize;
//            _charactersManagers = GameplayCoreManager.Instance.GameplayCharactersManager;
//        }
//    }
//}