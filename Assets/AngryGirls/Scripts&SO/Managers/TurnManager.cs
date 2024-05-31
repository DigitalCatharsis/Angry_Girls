using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Angry_Girls
{
    public enum CurrentPhase
    {
        AttackingPhase,
        LaunchingPhase,
    }

    public class TurnManager : MonoBehaviour
    {
        public int currentTurn = 0;
        public bool isLaunchingPhaseOver = false;
        public bool isAttackingPhaseOver = true;

        [SerializeField] CurrentPhase currentPhase = CurrentPhase.LaunchingPhase;

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

                currentPhase = CurrentPhase.AttackingPhase;
                isAttackingPhaseOver = false;

                //Adding enemies to attack list after 2 launches
                if (currentTurn == 1)
                {
                    var temp = _charactersTurn_List[_charactersTurn_List.Count-1];
                    _charactersTurn_List.RemoveAt(_charactersTurn_List.Count-1);
                    foreach (var character in Singleton.Instance.characterManager.enemyCharacters)
                    {

                        _charactersTurn_List.Add(character.gameObject);
                    }
                    _charactersTurn_List.Add(temp);
                }

                // wait untill everyone do its turn
                StartCoroutine(ProcessEachTurn_Routine());
            }

            if (currentPhase == CurrentPhase.AttackingPhase)
            {
                if (isAttackingPhaseOver == false)
                {
                    return;
                }
                ColorDebugLog.Log("Is AttackingPhase", System.Drawing.KnownColor.GhostWhite);
            }
        }

        private IEnumerator ProcessEachTurn_Routine()
        {            
            for (var i = 0; i < _charactersTurn_List.Count -1; i++)
            {
                Singleton.Instance.ñameraManager.CenterCameraAgainst(_charactersTurn_List[i].GetComponent<BoxCollider>());
                ColorDebugLog.Log(_charactersTurn_List[i].name.ToString(), KnownColor.Tan);
                yield return new WaitForSeconds(2);
            }
        }
    }
}