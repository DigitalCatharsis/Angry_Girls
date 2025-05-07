using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class TurnManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private float _timeBetweenTurns = 2f;

        private List<CControl> _turnOrder = new();
        private int _currentTurnIndex = 0;

        public void StartAlternatePhase(Action onComplete)
        {
            if (_turnOrder.Count == 0)
            {
                Debug.LogWarning("Turn order is empty!");
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(ExecuteTurnsRoutine(onComplete));
        }

        public void AddCharactersToTurnOrder(List<CControl> characters)
        {
            _turnOrder.AddRange(characters);
            SortTurnOrder();
        }

        private IEnumerator ExecuteTurnsRoutine(Action onComplete)
        {
            _currentTurnIndex = 0;

            while (_currentTurnIndex < _turnOrder.Count)
            {
                var currentCharacter = _turnOrder[_currentTurnIndex];

                if (!currentCharacter.isDead)
                {
                    yield return ExecuteCharacterTurn(currentCharacter);
                }

                _currentTurnIndex++;
            }

            onComplete?.Invoke();
        }

        private IEnumerator ExecuteCharacterTurn(CControl character)
        {
            _cameraManager.CameraFollowForRigidBody(character.CharacterMovement.Rigidbody);

            // Запускаем атаку персонажа
            character.isAttacking = true;
            yield return new WaitUntil(() => character.hasFinishedAlternateAttackTurn);

            yield return new WaitForSeconds(_timeBetweenTurns);
        }

        private void SortTurnOrder()
        {
            // Сначала игроки, потом враги
            _turnOrder.Sort((a, b) =>
                a.playerOrAi == PlayerOrAi.Character ? -1 : 1);

            // Удаляем мертвых персонажей
            _turnOrder.RemoveAll(c => c.isDead);
        }

        public void ClearTurnOrder()
        {
            _turnOrder.Clear();
        }
    }
}