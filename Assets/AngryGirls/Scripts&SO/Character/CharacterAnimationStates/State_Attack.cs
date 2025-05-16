namespace Angry_Girls
{
    public class State_Attack : AnimationStateBase
    {
        private AttackAbilityData _attackAbility;

        public State_Attack(CControl control, AnimationController animationController) : base(control, animationController) { }

        public override void OnEnter()
        {
            // attack init
            _attackAbility = GameLoader.Instance.gameFlowController.CurrentState == GameState.LaunchPhase
                ? _control.characterSettings.AttackAbility_Launch
                : _control.characterSettings.AttackAbility_Alternate;


            _control.isAttacking = true;
            _control.canUseAbility = false;
            _animationController.ChangeAnimationStateCrossFade(
                GameLoader.Instance.statesContainer.attack_Dictionary[_attackAbility.attack_State.animation],
                _attackAbility.attack_State.transitionDuration);
        }

        public override void OnExit()
        {
            _control.isAttacking = false;
        }
    }
}