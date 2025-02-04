using System.Collections.Generic;
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
        [SerializeField] private UICharModel[] _selectedCharactersHandlers;

        [Header("Debug")]
        [SerializeField] private List<UICharModel> _avaiblecharacterPoolHandlers;

        private void Start()
        {
            InitSelectedAndAvaible();
        }

        private void InitSelectedAndAvaible()
        {
            UpdateSelected();

            // Init avaible
            for (int i = 0; i < _playerData.avaibleCharacterPool.Count; i++)
            {
                if (_playerData.avaibleCharacterPool[i] == null)
                {
                    continue;
                }

                var _go = InstantiateUICharModel(avaibleCharactersGridTabPanel);
                //в инспекторе не заданы реакции на клик, передаем через делегат. (не будет отображаться в инспекторе)
                _go.GetComponentInChildren<Button>().onClick.AddListener(delegate { GetFromAvaible(_go.GetComponentInChildren<UICharModel>()); });
                _avaiblecharacterPoolHandlers.Add(_go.GetComponent<UICharModel>());

                _go.GetComponent<UICharModel>().UpdateElement(_playerData.avaibleCharacterPool, i); // Передаем массив и индекс
            }
        }

        private GameObject InstantiateUICharModel(GameObject GridTabPanel)
        {
            // Загружаем префаб из Resources
            var _prefab = Resources.Load("UICharModel") as GameObject;

            // Создаем экземпляр префаба
            var _go = Instantiate(_prefab);

            // Делаем его дочерним к avaibleCharactersGridTabPanel
            _go.transform.SetParent(GridTabPanel.transform, false);
            //_go.GetComponentInChildren<Button>().targetGraphic = null;
            return _go;
        }

        private void UpdateSelected()
        {
            // Init selected
            for (int i = 0; i < _playerData.selectedCharacters.Length; i++)
            {
                var elem = _selectedCharactersHandlers[i];
                elem.UpdateElement(_playerData.selectedCharacters, i); // Передаем массив и индекс
            }
        }

        private void UpdateAvaible()
        {
            int maxIndex;

            if (_playerData.avaibleCharacterPool.Count >= _avaiblecharacterPoolHandlers.Count)
            {
                maxIndex = _playerData.avaibleCharacterPool.Count;

                //спавним нужное колличество отсутсвующих Handler'ов
                for (int i = 0; i < _playerData.avaibleCharacterPool.Count - _avaiblecharacterPoolHandlers.Count; i++)
                {
                    var model = InstantiateUICharModel(avaibleCharactersGridTabPanel).GetComponent<UICharModel>();
                    _avaiblecharacterPoolHandlers.Add(model);
                    //в инспекторе не заданы реакции на клик, передаем через делегат. (не будет отображаться в инспекторе)
                    model.GetComponentInChildren<Button>().onClick.AddListener(delegate { GetFromAvaible(model); });
                }
            }
            else
            {
                maxIndex = _avaiblecharacterPoolHandlers.Count;
            }

            //add handlers if needed
            for (int i = 0; i < maxIndex; i++)
            {
                _avaiblecharacterPoolHandlers[i].UpdateElement(_playerData.avaibleCharacterPool, i);
            }
        }

        public void RemoveFromSelected(UICharModel uICharModel)
        {
            if (uICharModel.Index < 0)
            {
                return;
            }

            Debug.Log("Removing from selected");
            _playerData.RemoveFromSelected(uICharModel.Index);
            UpdateSelected();
            UpdateAvaible();
        }

        public void GetFromAvaible(UICharModel uICharModel)
        {
            if(uICharModel.Index < 0)
            {
                return;
            }

            if (_playerData.GetFromAvaible(uICharModel.Index))
            {
                UpdateSelected();
                UpdateAvaible();
            }
        }

        public void ChangeTab(GameObject selectedTab)
        {
            currentTab.SetActive(false);
            selectedTab.SetActive(true);
        }
    }
}