using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class State_Idle : AnimationStateBase
    {
        public State_Idle(CControl control) : base(control) { }

        private GameplayCharactersManager _charactersManager;

        public override void OnEnter()
        {
            if (_charactersManager == null)
            {
                _charactersManager = GameplayCoreManager.Instance.GameplayCharactersManager;
            }

            var idleState = _settings.GetRandomState(_settings.idle_States);
            AnimationTransitioner.ChangeAnimationStateCrossFade
                (
                _control.animator,
                   GameplayCoreManager.Instance.StatesContainer.idle_Dictionary[idleState.animation],
                   idleState.transitionDuration
                );
        }

        public override void OnUpdate()
        {
            if (_control.playerOrAi == PlayerOrAi.Player && _control.IsInIdleState())
            {
                TurnToTheClosestEnemy(PlayerOrAi.Bot);
            }
            else if (_control.playerOrAi == PlayerOrAi.Bot)
            {
                TurnToTheClosestEnemy(PlayerOrAi.Player);
            }
        }

        private void TurnToTheClosestEnemy(PlayerOrAi typeOfUnitToTurn)
        {
            float closestDistance = 9999f;
            var collection = new List<CControl>();

            collection = _charactersManager.GetAliveCharacters(typeOfUnitToTurn);

            foreach (var character in collection)
            {
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