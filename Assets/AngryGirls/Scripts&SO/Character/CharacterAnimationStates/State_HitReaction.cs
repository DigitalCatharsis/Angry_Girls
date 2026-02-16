namespace Angry_Girls
{
    public class State_HitReaction : AnimationStateBase
    {
        public State_HitReaction(CControl control, AnimationController animationController)
            : base(control, animationController) { }

        public override void OnEnter()
        {
            _control.unitGotHit = true;
            _control.isAttacking = false;
            _control.isLanding = false;
            _control.unitGotHit = false;


            var randomHitAnimation = _settings.GetRandomState(_settings.hitReaction_States).animation;
            _animationController.ChangeAnimationState(
                GameplayCoreManager.Instance.StatesContainer.hitReaction_Dictionary[randomHitAnimation],
                transitionDuration: 0.1f);
        }

        public override void OnExit()
        {
            _control.unitGotHit = false;
        }
    }
}