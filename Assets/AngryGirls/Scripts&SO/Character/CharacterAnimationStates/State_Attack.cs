namespace Angry_Girls
{
    public class State_Attack : AnimationStateBase
    {
        private AttackAbilityData _attackAbility;

        public State_Attack(CControl control) : base(control) { }

        public override void OnEnter()
        {
            _attackAbility = GameplayCoreManager.Instance.PhaseFlowController.CurrentState == GameState.LaunchPhase
                ? _control.profile.CharacterSettings.AttackAbility_Launch
                : _control.profile.CharacterSettings.AttackAbility_Alternate;

            _control.isAttacking = true;
            _control.canUseAbility = false;

            AnimationTransitioner.ChangeAnimationStateCrossFade(
                _control.animator,
                GameplayCoreManager.Instance.StatesContainer.attack_Dictionary[_attackAbility.attack_State.animation],
                _attackAbility.attack_State.transitionDuration);
        }

        public override void OnExit()
        {
            _control.isAttacking = false;
        }
    }
}