using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class State_Idle : AnimationStateBase
    {
        public State_Idle(CControl control, AnimationController animationController): base(control, animationController) { }

        public override void OnEnter()
        {
            var idleState = _settings.GetRandomState(_settings.idle_States);
            _animationController.ChangeAnimationStateCrossFade
                (
                   GameLoader.Instance.statesContainer.idle_Dictionary[idleState.animation],
                   idleState.transitionDuration
                );
        }

        public override void OnUpdate()
        {
            if (_control.playerOrAi == PlayerOrAi.Player && _control.hasFinishedLaunchingTurn)
            {
                TurnToTheClosestEnemy(PlayerOrAi.Ai);
            }
            else if (_control.playerOrAi == PlayerOrAi.Ai)
            {
                TurnToTheClosestEnemy(PlayerOrAi.Player);
            }
        }

        private void TurnToTheClosestEnemy(PlayerOrAi playerOrAi)
        {
            float closestDistance = 9999f;
            var collection = new List<CControl>();

            if (playerOrAi == PlayerOrAi.Player)
            {
                collection = GameLoader.Instance.characterManager.playableCharacters;
            }
            else
            {
                collection = GameLoader.Instance.characterManager.enemyCharacters;
            }

                foreach (var character in collection)
            {
                if (character.GetComponent<CControl>().isDead) continue;

                var distance = _control.CharacterMovement.Rigidbody.position.z -
                               character.CharacterMovement.Rigidbody.position.z;

                if (Math.Abs(closestDistance) > Math.Abs(distance))
                {
                    closestDistance = distance;
                }
            }

            if (closestDistance > 0)
            {
                _control.CharacterMovement.SetRotation(Quaternion.Euler(0, 180, 0));
            }
            else if (closestDistance < 0)
            {
                _control.CharacterMovement.SetRotation(Quaternion.Euler(0, 0, 0));
            }
        }
    }
}