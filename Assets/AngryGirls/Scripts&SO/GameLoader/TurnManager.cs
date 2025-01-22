using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public enum CurrentPhase
    {
        AlternatePhase,
        LaunchingPhase,
    }

    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private int _currentTurn = 0;
        [SerializeField] private CurrentPhase _currentPhase = CurrentPhase.LaunchingPhase;

        [SerializeField] private bool _isLaunchingPhaseOver = false;
        [SerializeField] private bool _isAlternatePhaseOver = true;

        private const float _timeToWentAfterUnitFinishedAttack = 2f;
        public float TimeToChangePhase { get => _timeToWentAfterUnitFinishedAttack; }

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

                //current Phase is Alternate
                SwitchToAttackingPhase();

                // wait untill everyone do its turn then switch to LaunchimgPhase
                StartCoroutine(OnEachTurn_Routine());
            }

            if (_currentPhase == CurrentPhase.AlternatePhase)
            {
                if (_isAlternatePhaseOver == false)
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

                //TODO: camera has to follow in Alternate state about several frames? so make a corutine?
                GameLoader.Instance.cameraManager.FollowCamera(_charactersTurn_List[i]);
                ColorDebugLog.Log(_charactersTurn_List[i].name.ToString() + " is attacking.", KnownColor.Aqua);

                //Attack
                _charactersTurn_List[i].GetComponent<CControl>().isAttacking = true;

                yield return new WaitForSeconds(_timeToWentAfterUnitFinishedAttack);
            }

            GameLoader.Instance.cameraManager.ReturnCameraToStartPosition(1f);
            SwitchToLaunchingPhase();
        }

        private void SwitchToAttackingPhase()
        {
            _isLaunchingPhaseOver = true;
            _isAlternatePhaseOver = false;
            _currentPhase = CurrentPhase.AlternatePhase;
            foreach (var character in _charactersTurn_List)
            {
                var control = character.GetComponent<CControl>();
                control.hasFinishedAlternateAttackTurn = false;
                control.canUseAbility = true;
                control.hasUsedAbility = false;
            }
        }

        private void SwitchToLaunchingPhase()
        {
            _currentTurn++;
            _isAlternatePhaseOver = true;
            SortCharactersTurnList();
            _currentPhase = CurrentPhase.LaunchingPhase;
            _isLaunchingPhaseOver = false;
            GameLoader.Instance.launchManager.Allow_CharacterPress();
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
                if (character.GetComponent<CControl>().playerOrAi == PlayerOrAi.Player)
                {
                    tempCharacters.Add(character);
                }
                else if (character.GetComponent<CControl>().playerOrAi == PlayerOrAi.Ai)
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