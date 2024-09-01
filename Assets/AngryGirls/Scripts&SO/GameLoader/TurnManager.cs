using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public enum CurrentPhase
    {
        StaticPhase,
        LaunchingPhase,
    }

    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private int _currentTurn = 0;
        [SerializeField] private CurrentPhase _currentPhase = CurrentPhase.LaunchingPhase;

        [SerializeField] private bool _isLaunchingPhaseOver = false;
        [SerializeField] private bool _isStaticPhaseOver = true;

        [SerializeField] private List<GameObject> _charactersTurn_List = new();

        //props
        public int CurrentTurn => _currentTurn;
        public bool IsLaunchingPhaseOver { set => _isLaunchingPhaseOver = value; }
        public CurrentPhase CurrentPhase => _currentPhase;

        public void IncrementCurentTurn()
        {
            _currentTurn++;
        }

        public void AddCharacterToTurnList(GameObject character)
        {
            _charactersTurn_List.Add(character);
        }

        private void Update()
        {
            if (_currentPhase == CurrentPhase.LaunchingPhase)
            {                
                if (_isLaunchingPhaseOver == false)
                {
                    return;
                }


                //Adding enemies to attack list after 2 launches
                if (_currentTurn == 1)
                {
                    _charactersTurn_List.InsertRange(_charactersTurn_List.Count - 1, GameLoader.Instance.characterManager.enemyCharacters);
                }

                //current Phase is Static
                SwitchToAttackingPhase();

                // wait untill everyone do its turn then switch to LaunchimgPhase
                StartCoroutine(OnEachTurn_Routine());
            }

            if (_currentPhase == CurrentPhase.StaticPhase)
            {
                if (_isStaticPhaseOver == false)
                {
                    return;
                }
            }
        }

        private IEnumerator OnEachTurn_Routine()
        {            
            for (var i = 0; i < _charactersTurn_List.Count -1; i++)
            {
                if (_charactersTurn_List[i].GetComponent<CControl>().isDead == true)
                {
                    continue;
                }

                GameLoader.Instance.cameraManager.CenterCameraAgainst(_charactersTurn_List[i].GetComponent<BoxCollider>());
                ColorDebugLog.Log(_charactersTurn_List[i].name.ToString() + " is attacking.", KnownColor.Aqua);

                //Attack
                _charactersTurn_List[i].GetComponent<CControl>().isAttacking = true;

                yield return new WaitForSeconds(2);
            }

            GameLoader.Instance.cameraManager.ReturnCameraToStartPosition(1f);
            SwitchToLaunchingPhase();
        }

        private void SwitchToAttackingPhase()
        {
            _isLaunchingPhaseOver = true;
            _isStaticPhaseOver = false;
            _currentPhase = CurrentPhase.StaticPhase;
            foreach (var character in _charactersTurn_List)
            {
                character.GetComponent<CControl>().hasFinishedStaticAttackTurn = false;
            }
        }

        private void SwitchToLaunchingPhase()
        {
            _currentTurn++;
            _isStaticPhaseOver = true;
            SortCharactersTurnList();
            _currentPhase = CurrentPhase.LaunchingPhase;
            _isLaunchingPhaseOver = false;
            GameLoader.Instance.gameLoaderMediator.Notify(this, GameLoaderMediator_EventNames.AllowCharacterPress);
            //Singleton.Instance.launchManager.canPressAtCharacters = true;
        }

        private void SortCharactersTurnList()
        {
            var tempCharacters = new List<GameObject>();
            var tempEnemies = new List<GameObject>();

            foreach ( var character in _charactersTurn_List )
            {
                if (character.GetComponent<CControl>().isDead)
                {
                    continue;
                }
                if (character.GetComponent<CharacterControl>())
                {
                    tempCharacters.Add(character);
                }
                else if (character.GetComponent<EnemyControl>())
                {
                    tempEnemies.Add(character);
                }
            }
            _charactersTurn_List.Clear();
            _charactersTurn_List.AddRange(tempCharacters);
            _charactersTurn_List.AddRange(tempEnemies);
        }
    }
}