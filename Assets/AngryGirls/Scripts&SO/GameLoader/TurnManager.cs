using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private float _timeToWentAfterUnitFinishedAttack = 2f;
        [SerializeField] private List<CControl> _charactersTurn_List = new();

        public IEnumerator AlternatePhaseRoutine(System.Action onComplete)
        {
            SortCharactersTurnList();
            _charactersTurn_List.RemoveAll(c => c == null || c.isDead);

            foreach (var character in _charactersTurn_List)
            {
                GameLoader.Instance.cameraManager.CameraFollowForRigidBody(character.CharacterMovement.Rigidbody);
                character.isAttacking = true;
                yield return new WaitForSeconds(_timeToWentAfterUnitFinishedAttack);
            }

            GameLoader.Instance.cameraManager.ReturnCameraToStartPosition(1f);
            onComplete?.Invoke();
        }

        private void SortCharactersTurnList()
        {
            var tempCharacters = new List<CControl>();
            var tempEnemies = new List<CControl>();

            foreach (var character in _charactersTurn_List)
            {
                if (character == null || character.isDead) continue;

                if (character.playerOrAi == PlayerOrAi.Character)
                    tempCharacters.Add(character);
                else
                    tempEnemies.Add(character);
            }

            _charactersTurn_List.Clear();
            _charactersTurn_List.AddRange(tempCharacters);
            _charactersTurn_List.AddRange(tempEnemies);
        }

        public void AddCharacterToTurnList(CControl character)
        {
            _charactersTurn_List.Add(character);
        }
    }
}