using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Manages turn order execution during alternate attack phase
    /// </summary>
    public class TurnManager : GameplayManagerClass
    {
        [SerializeField] private float _timeToWaitAfterUnitFinishedAttack = 0.7f;

        private CameraManager _cameraManager;
        private GameplayCharactersManager _charactersManager;
        private List<CControl> _currentTurnOrder = new();

        public override void Initialize()
        {
            _cameraManager = GameplayCoreManager.Instance.CameraManager;
            _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;
            isInitialized = true;
        }

        public void ExecuteAlternatePhase(System.Action onComplete)
        {
            StartCoroutine(AlternatePhaseRoutine(onComplete));
        }

        private IEnumerator AlternatePhaseRoutine(System.Action onComplete)
        {
            if (!isInitialized) yield break;

            BuildTurnOrder();
            RemoveDeadCharacters();  
            ReinitCharactersForAlternatePhase(); 

            yield return ExecuteTurnsSequentially(); 

            MoveCameraToLauncherPosition();
            onComplete?.Invoke();
        }

        private void BuildTurnOrder()
        {
            // Get ALL alive characters
            var allAlive = new List<CControl>();
            allAlive.AddRange(_charactersManager.GetAliveCharacters(PlayerOrAi.Player));
            allAlive.AddRange(_charactersManager.GetAliveCharacters(PlayerOrAi.Bot));

            // Filter by mechanics:
            // 1. Enemies: ALWAYS move in AlternatePhase
            // 2. Players: move ONLY if launched AND NOT the last launched
            _currentTurnOrder = allAlive.FindAll(c =>
            (c.playerOrAi == PlayerOrAi.Bot) || // All bots move
            (c.playerOrAi == PlayerOrAi.Player &&
            c.hasBeenLaunched &&
            c != GameplayCoreManager.Instance.LaunchManager.LastLaunchedCharacter) // Launched players except the last launched
            );

            // Grouping: all players first, then all enemies (as in the old working code)
            _currentTurnOrder.Sort((a, b) =>
            {
                // Players always before enemies
                if (a.playerOrAi == PlayerOrAi.Player && b.playerOrAi == PlayerOrAi.Bot) return -1;
                if (a.playerOrAi == PlayerOrAi.Bot && b.playerOrAi == PlayerOrAi.Player) return 1;

                return 0;
            });
        }

        private void RemoveDeadCharacters()
        {
            _currentTurnOrder.RemoveAll(c => c == null || c.isDead);
        }

        private void ReinitCharactersForAlternatePhase()
        {
            foreach (var character in _currentTurnOrder)
            {
                character.unitBehaviorIsAlternate = true;
                character.hasFinishedLaunchingTurn = true;
                character.hasFinishedAlternateAttackTurn = false; 
                character.canUseAbility = true;
                character.hasUsedAbility = false;
            }
        }

        private IEnumerator ExecuteTurnsSequentially()
        {
            for (int index = 0; index < _currentTurnOrder.Count; index++)
            {
                if (_currentTurnOrder[index].isDead)
                    continue;

                yield return ExecuteSingleTurn(_currentTurnOrder[index]);
            }
        }

        private IEnumerator ExecuteSingleTurn(CControl character)
        {
            _cameraManager.CameraFollowForRigidBody(character.CharacterMovement.Rigidbody);
            _charactersManager.CurrentlyAttackingUnit = character;

            character.isAttacking = true;
            yield return new WaitWhile(() => character.isAttacking && !character.isDead);
            yield return new WaitForSeconds(_timeToWaitAfterUnitFinishedAttack);

            character.hasFinishedAlternateAttackTurn = true; 
            _charactersManager.CurrentlyAttackingUnit = null;
        }

        private void MoveCameraToLauncherPosition()
        {
            var launcher = GameplayCoreManager.Instance.StageManager.CurrentCharacterLauncher;
            _cameraManager.MoveCameraTo(
                new Vector3(Camera.main.transform.position.x, launcher.transform.position.y, launcher.transform.position.z),
                0.5f,
                true
            );
        }

        public void ClearTurnList()
        {
            _currentTurnOrder.Clear();
        }
    }
}