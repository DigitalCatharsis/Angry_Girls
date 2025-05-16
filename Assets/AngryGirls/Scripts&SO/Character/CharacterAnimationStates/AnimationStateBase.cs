using UnityEngine;

namespace Angry_Girls
{
    public interface IAnimationState
    {
        public void OnEnter();
        public void OnUpdate();
        public void OnExit();
        public bool CanTransitionTo(IAnimationState nextState);
    }

    public abstract class AnimationStateBase : IAnimationState
    {
        protected readonly CControl _control;
        protected readonly Animator _animator;
        protected readonly AnimationController _animationController;
        protected readonly CharacterSettings _settings;

        protected AnimationStateBase(CControl control, AnimationController animationController)
        {
            _control = control;
            _animator = control.animator;
            _animationController = animationController;
            _settings = control.characterSettings;
        }
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnExit() { }

        public virtual bool CanTransitionTo(IAnimationState nextState)
        {
            return true; // ѕо умолчанию разрешаем все переходы
        }
    }
}