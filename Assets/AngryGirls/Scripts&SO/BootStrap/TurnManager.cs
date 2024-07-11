using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public enum CurrentPhase
    {
        StaticAttackingPhase,
        LaunchingPhase,
    }

    public class TurnManager : MonoBehaviour
    {
        public int currentTurn = 0;
        public bool isLaunchingPhaseOver = false;
        public bool isAttackingPhaseOver = true;

        public CurrentPhase currentPhase = CurrentPhase.LaunchingPhase;

        [SerializeField] private List<GameObject> _charactersTurn_List = new();

        public void AddCharacterToTurnList(GameObject character)
        {
            _charactersTurn_List.Add(character);
        }

        public void RemoveCharacterFromTurnList(GameObject character)
        {
            _charactersTurn_List.Remove(character);
        }

        private void Update()
        {
            if (currentPhase == CurrentPhase.LaunchingPhase)
            {                
                if (isLaunchingPhaseOver == false)
                {
                    return;
                }

                SwitchToAttackingPhase();

                //Adding enemies to attack list after 2 launches
                if (currentTurn == 1)
                {
                    _charactersTurn_List.InsertRange(_charactersTurn_List.Count - 1, Singleton.Instance.characterManager.enemyCharacters);
                }

                // wait untill everyone do its turn then switch to LaunchimgPhase
                StartCoroutine(OnEachTurn_Routine());
            }

            if (currentPhase == CurrentPhase.StaticAttackingPhase)
            {
                if (isAttackingPhaseOver == false)
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

                Singleton.Instance.ñameraManager.CenterCameraAgainst(_charactersTurn_List[i].GetComponent<BoxCollider>());
                ColorDebugLog.Log(_charactersTurn_List[i].name.ToString() + " is attacking.", KnownColor.Aqua);

                //Attack

                //_charactersTurn_List[i].GetComponent<CControl>().animator.Play("A_Shoryuken_DownSmash_Finish", 0);
                _charactersTurn_List[i].GetComponent<CControl>().isAttacking = true;

                yield return new WaitForSeconds(2);
            }
            Singleton.Instance.ñameraManager.ReturnCameraToStartPosition(1f);
            SwitchToLaunchingPhase();
        }

        private void SwitchToAttackingPhase()
        {
            isLaunchingPhaseOver = true;
            isAttackingPhaseOver = false;
            currentPhase = CurrentPhase.StaticAttackingPhase;
            foreach (var character in _charactersTurn_List)
            {
                character.GetComponent<CControl>().subComponentProcessor.attackSystem.hasFinishedStaticAttackTurn = false;
            }
        }

        private void SwitchToLaunchingPhase()
        {
            Singleton.Instance.turnManager.currentTurn++;
            isAttackingPhaseOver = true;
            SortCharactersTurnList();
            currentPhase = CurrentPhase.LaunchingPhase;
            isLaunchingPhaseOver = false;
            Singleton.Instance.launchManager.canPressAtCharacters = true;
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