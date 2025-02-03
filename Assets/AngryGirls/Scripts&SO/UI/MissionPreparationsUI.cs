using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class MissionPreparationsUI : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private GameObject currentTab;
        [SerializeField] private GameObject selectedCharactersGridTabPanel;
        [SerializeField] private GameObject avaibleCharactersGridTabPanel;
        [Space(10)]
        [SerializeField] private PlayerData _playerData;
        [Space(10)]
        [SerializeField] private GameObject[] _selectedCharactersHandlers;

        [Header("Debug")]
        [SerializeField] private GameObject[] _characterPoolHandlers;


        private void Start()
        {
            InitSelectedAndAvaible();
        }

        private void InitSelectedAndAvaible()
        {
            //init selected
            for (int i = 0; i < _playerData.selectedCharacters.Count; i++)
            {
                var model = _selectedCharactersHandlers[i].GetComponent<UICharacterModel>();
                model.characterType = _playerData.selectedCharacters[i].characterType;
                var test = model.gameObject.GetComponentInChildren<Image>(); 
                test.sprite = _playerData.selectedCharacters[i].portrait;
            }

            //init avaible
            for (int i = 0; i < _playerData.avaibleCharacterPool.Count; i++)
            {

            }
        }

        public void RemoveCharToAvaiblePool(CharacterType character)
        {
        }

        public void AddCharToList(CharacterType selectedCharacter)
        {
            //var currentCharacters = _playerData.selectedCharacters;

            //if (currentCharacters.Length == 4)
            //{
            //    return;
            //}

            //for (int i = 0; i < currentCharacters.Length; i++)
            //{
            //    if (currentCharacters[i] == CharacterType.NULL)
            //    {
            //        currentCharacters[i] = selectedCharacter;
            //    }
            //}
        }

        public void ChangeTab(GameObject selectedTab)
        {
            currentTab.SetActive(false);
            selectedTab.SetActive(true);
        }
    }
}
