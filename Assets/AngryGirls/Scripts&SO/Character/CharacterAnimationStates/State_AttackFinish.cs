namespace Angry_Girls
{
    public class State_AttackFinish : AnimationStateBase
    {
        public State_AttackFinish(CControl control)
            : base(control) { }

        public override void OnEnter()
        {
            var attackFinishState = _control.Get_AttackAbility().attackFininsh_State;
            AnimationTransitioner.ChangeAnimationStateFixedTime(
                _control.animator,
                GameplayCoreManager.Instance.StatesContainer.attackFinish_Dictionary[attackFinishState.animation],
                attackFinishState.transitionDuration);

            _control.isAttacking = false;
        }

        public override void OnUpdate()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
            }
        }

        public override void OnExit()
        {
        }

        public override bool CanTransitionTo(IAnimationState nextState)
        {
            return nextState is State_Idle || nextState is State_HitReaction;
        }
    }
}