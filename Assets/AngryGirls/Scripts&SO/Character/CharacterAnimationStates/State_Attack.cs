namespace Angry_Girls
{
    public class State_Attack : AnimationStateBase
    {
        private AttackAbilityData _attackAbility;

        public State_Attack(CControl control, AnimationController animationController) : base(control, animationController) { }

        public override void OnEnter()
        {
            // attack init
            _attackAbility = GameplayCoreManager.Instance.GameFlowController.CurrentState == GameState.LaunchPhase
                ? _control.profile.CharacterSettings.AttackAbility_Launch
                : _control.profile.CharacterSettings.AttackAbility_Alternate;


            _control.isAttacking = true;
            _control.canUseAbility = false;
            _animationController.ChangeAnimationStateCrossFade(
                GameplayCoreManager.Instance.StatesContainer.attack_Dictionary[_attackAbility.attack_State.animation],
                _attackAbility.attack_State.transitionDuration);
        }

        public override void OnExit()
        {
            _control.isAttacking = false;
        }
    }
}