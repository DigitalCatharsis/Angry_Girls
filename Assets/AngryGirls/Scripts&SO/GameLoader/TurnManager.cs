using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Angry_Girls
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private float _timeToWentAfterUnitFinishedAttack = 0.7f;
        [SerializeField] private List<CControl> _charactersTurn_List = new();
        public CControl currentAttackingUnit;

        public void ClearTurnList()
        {
            _charactersTurn_List.Clear();
        }

        public IEnumerator AlternatePhaseRoutine(System.Action onComplete)
        {
            SortCharactersTurnList();
            _charactersTurn_List.RemoveAll(c => c == null || c.isDead);

            ReinitCharactersStats(_charactersTurn_List);

            for (int index = 0; index < _charactersTurn_List.Count; index++)
            {
                if (_charactersTurn_List[index].isDead)
                {
                    continue;
                }

                // Проверяем существование следующего элемента в списке
                if ((index + 1) < _charactersTurn_List.Count &&
                    _charactersTurn_List[index].playerOrAi == PlayerOrAi.Character &&
                    _charactersTurn_List[index + 1].playerOrAi == PlayerOrAi.Bot)
                {
                    // Выполняем нужное действие
                    continue;
                }

                GameLoader.Instance.cameraManager.CameraFollowForRigidBody(_charactersTurn_List[index].CharacterMovement.Rigidbody);
                currentAttackingUnit = _charactersTurn_List[index];
                _charactersTurn_List[index].isAttacking = true;
                yield return new WaitWhile(() => _charactersTurn_List[index].isAttacking);
                yield return new WaitForSeconds(_timeToWentAfterUnitFinishedAttack);
                currentAttackingUnit = null;
            }

            var launcher = GameLoader.Instance.stageManager.CurrentCharacterLauncher;
            GameLoader.Instance.cameraManager.MoveCameraTo(new Vector3(Camera.main.transform.position.x, launcher.transform.position.y, launcher.transform.position.z), 0.5f, true);
            onComplete?.Invoke();
        }

        private void ReinitCharactersStats(List<CControl> controls)
        {
            foreach (var character in _charactersTurn_List)
            {
                character.unitBehaviorIsAlternate = true;
                character.hasFinishedLaunchingTurn = true;
                character.hasFinishedAlternateAttackTurn = false;
                character.canUseAbility = true;
                character.hasUsedAbility = false;
            }
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