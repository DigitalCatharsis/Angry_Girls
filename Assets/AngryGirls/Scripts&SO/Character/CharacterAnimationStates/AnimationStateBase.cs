using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Interface for animation states in the state machine
    /// </summary>
    public interface IAnimationState
    {
        public void OnEnter();
        public void OnUpdate();
        public void OnExit();
        public bool CanTransitionTo(IAnimationState nextState);
    }

    /// <summary>
    /// Base class for all animation states
    /// </summary>
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
            _settings = control.CharacterSettings;
        }

        /// <summary>
        /// Called when entering the state
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// Called every frame while in the state
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Called when exiting the state
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// Determines if transition to next state is allowed
        /// </summary>
        /// <param name="nextState">State to transition to</param>
        /// <returns>True if transition is allowed</returns>
        public virtual bool CanTransitionTo(IAnimationState nextState)
        {
            return true;
        }
    }
}