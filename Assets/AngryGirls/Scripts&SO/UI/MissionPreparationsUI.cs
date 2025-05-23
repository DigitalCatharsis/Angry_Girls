using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class MissionPreparationsUI : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private GameObject currentTab;
        [SerializeField] private GameObject selectedCharactersGridTabPanel;
        [SerializeField] private GameObject avaibleCharactersGridTabPanel;
        [SerializeField] private Button readyButton;
        [Space(10)]
        [Space(10)]
        [SerializeField] private UICharModel[] _selectedCharactersHandlers;

        [Header("Debug")]
        [SerializeField] private List<UICharModel> _avaiblecharacterPoolHandlers;
        private PlayerData _playerData;

        private int _credits;
        private void Start()
        {
            _playerData = GameLoader.Instance.levelSettings.playerData;
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
                //� ���������� �� ������ ������� �� ����, �������� ����� �������. (�� ����� ������������ � ����������)
                _go.GetComponentInChildren<Button>().onClick.AddListener(delegate { GetFromAvaible(_go.GetComponentInChildren<UICharModel>()); });
                _avaiblecharacterPoolHandlers.Add(_go.GetComponent<UICharModel>());

                _go.GetComponent<UICharModel>().UpdateElement(_playerData.avaibleCharacterPool, i); // �������� ������ � ������
            }
        }

        private GameObject InstantiateUICharModel(GameObject GridTabPanel)
        {
            // ��������� ������ �� Resources
            var _prefab = Resources.Load("UICharModel") as GameObject;

            // ������� ��������� �������
            var _go = Instantiate(_prefab);

            _go.transform.localScale = Vector3.one * 0.85f;
            // ������ ��� �������� � avaibleCharactersGridTabPanel
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
                elem.UpdateElement(_playerData.selectedCharacters, i); // �������� ������ � ������
            }
        }

        private void Update()
        {
            if (_playerData.selectedCharacters[0] != null)
            {
                readyButton.interactable = true;
            }
            else 
            { 
                readyButton.interactable = false;
            }
        }

        public void NewGame_Ready()
        {
            // �������� ������ ������� �����
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // ��������� ��������� �����
            SceneManager.LoadScene(currentSceneIndex + 1);
            //SceneManager.LoadScene(1);
        }

        private void UpdateAvaible()
        {
            int maxIndex;

            if (_playerData.avaibleCharacterPool.Count >= _avaiblecharacterPoolHandlers.Count)
            {
                maxIndex = _playerData.avaibleCharacterPool.Count;

                //������� ������ ����������� ������������ Handler'��
                for (int i = 0; i < _playerData.avaibleCharacterPool.Count - _avaiblecharacterPoolHandlers.Count; i++)
                {
                    var model = InstantiateUICharModel(avaibleCharactersGridTabPanel).GetComponent<UICharModel>();
                    _avaiblecharacterPoolHandlers.Add(model);
                    //� ���������� �� ������ ������� �� ����, �������� ����� �������. (�� ����� ������������ � ����������)
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


        public void AddCredits(int amount)
        {
            _credits += amount;
            UpdateCreditsUI();
        }

        public bool SpendCredits(int amount)
        {
            if (_credits < amount) return false;
            _credits -= amount;
            UpdateCreditsUI();
            return true;
        }

        private void UpdateCreditsUI()
        {
            // TextMeshPro ����������
        }

    }
}