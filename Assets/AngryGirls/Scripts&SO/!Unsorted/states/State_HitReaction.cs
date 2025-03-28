using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class State_HitReaction : AnimationStateBase
    {
        public State_HitReaction(CControl control, AnimationController animationController)
            : base(control, animationController) { }

        public override void OnEnter()
        {
            _control.unitGotHit = true;
            var randomHitAnimation = _settings.GetRandomState(_settings.hitReaction_States).animation;
            _animationController.ChangeAnimationState(
                GameLoader.Instance.statesContainer.hitReaction_Dictionary[randomHitAnimation],
                transitionDuration: 0.1f);
        }

        public override void OnExit()
        {
            _control.unitGotHit = false;
        }
    }
}