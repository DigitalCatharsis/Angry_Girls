namespace Angry_Girls
{
    public class State_HitReaction : AnimationStateBase
    {
        public State_HitReaction(CControl control, AnimationController animationController)
            : base(control, animationController) { }

        public override void OnEnter()
        {
            ColorDebugLog.Log($"{_control.name} has entered {_control.GetCurrentAnimationName()}. Settings isAttacking to false. And unitGotHit to true (on enter hit animation)", System.Drawing.KnownColor.Aqua);
            _control.unitGotHit = false;
            _control.isAttacking = false;
            _control.isLanding = false;


            var randomHitAnimation = _settings.GetRandomState(_settings.hitReaction_States).animation;
            _animationController.ChangeAnimationState(
                GameplayCoreManager.Instance.StatesContainer.hitReaction_Dictionary[randomHitAnimation],
                transitionDuration: 0.1f);
        }

        public override void OnExit()
        {
            ColorDebugLog.Log($"{_control.name} has entered OnExit from" +
                $" {_control.GetCurrentAnimationName()}. Settings unitGotHit to false (on exit hit animation)", System.Drawing.KnownColor.Aqua);
            _control.unitGotHit = false;
        }
    }
}