using UnityEngine;

namespace Angry_Girls
{
    public class State_AttackFinish : AnimationStateBase
    {
        public State_AttackFinish(CControl control, AnimationController animationController)
            : base(control, animationController) { }

        public override void OnEnter()
        {
            var attackFinishState = _control.Get_AttackAbility().attackFininsh_State;
            _animationController.ChangeAnimationStateFixedTime(
                GameLoader.Instance.statesContainer.attackFinish_Dictionary[attackFinishState.animation],
                attackFinishState.transitionDuration);

            _control.isAttacking = false;
        }

        public override void OnUpdate()
        {
            //TODO:!
            // ћожно добавить проверку завершени€ анимации
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                //_stateMachine.ChangeState<State_Idle>();
            }
        }

        public override void OnExit()
        {
            _control.FinishTurn();
        }

        public override bool CanTransitionTo(IAnimationState nextState)
        {
            // –азрешаем переход только в idle или при получении удара
            return nextState is State_Idle || nextState is State_HitReaction;
        }
    }
}