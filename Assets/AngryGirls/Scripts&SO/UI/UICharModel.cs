using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class UICharModel : MonoBehaviour
    {
        [SerializeField] private int _playerData_Index = -1;

        public int Index { get => _playerData_Index; }

        public void UpdateElement(List<CharacterSettings> characterSettingsArray, int index)
        {
            if (characterSettingsArray.Count <= index)
            {
                NullValues();
                return;
            }
            if (index < 0 || characterSettingsArray[index] == null)
            {
                NullValues();
                return;
            }

            _playerData_Index = index;

            var _characterSettings = characterSettingsArray[index];
            GetComponentInChildren<Image>().sprite = _characterSettings.portrait;
            GetComponentInChildren<Button>().interactable = true;
        }

        public void UpdateElement(CharacterSettings[] characterSettingsArray, int index)
        {
            if (index < 0 | characterSettingsArray[index] == null)
            {
                NullValues();
                return;
            }


            _playerData_Index = index;

            var _characterSettings = characterSettingsArray[index];
            GetComponentInChildren<Image>().sprite = _characterSettings.portrait;
            GetComponentInChildren<Button>().interactable = true;
        }

        public void NullValues()
        {
            _playerData_Index = -1;
            GetComponentInChildren<Button>().interactable = false;
            GetComponentInChildren<Image>().sprite = null;
        }
    }
}