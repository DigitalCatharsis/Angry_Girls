using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Controller for managing animation playback and transitions
    /// </summary>
    public class AnimationController
    {
        public AnimationController(GameObject owner, Animator animator, HashManager hashManager, StatesContainer statesContainer)
        {
            _owner = owner;
            _animator = animator;
            _hashManager = hashManager;
            _statesContainer = statesContainer;
        }

        private GameObject _owner;
        private readonly Animator _animator;
        private readonly HashManager _hashManager;
        private readonly StatesContainer _statesContainer;

        /// <summary>
        /// Changes animation state with Play method
        /// </summary>
        public void ChangeAnimationState(int newStateHash, float transitionDuration = 0.1f, int layer = 0)
        {
            if (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == newStateHash) return;
            _animator.Play(newStateHash, layer, transitionDuration);
        }

        /// <summary>
        /// Changes animation state with PlayInFixedTime method
        /// </summary>
        public void ChangeAnimationStateFixedTime(int newStateHash, float transitionDuration = 0f, int layer = 0)
        {
            if (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == newStateHash) return;
            _animator.PlayInFixedTime(newStateHash, layer, transitionDuration);
        }

        /// <summary>
        /// Changes animation state with cross-fade
        /// </summary>
        public void ChangeAnimationStateCrossFade(int newStateHash, float transitionDuration = 0.1f, int layer = 0)
        {
            var currentHash = _animator.GetCurrentAnimatorStateInfo(layer).shortNameHash;

            // Don't transition to NONE states
            if (_hashManager.GetName(_statesContainer.stateNames_Dictionary, newStateHash) == StateNames.NONE) return;

            _animator.CrossFadeInFixedTime(
                newStateHash,
                fixedTransitionDuration: transitionDuration,
                layer: layer,
                fixedTimeOffset: 0f,
                normalizedTransitionTime: 0.0f);
        }
    }

    /// <summary>
    /// Finite State Machine for animation states
    /// </summary>
    public class AnimationStateMachine
    {
        public AnimationStateMachine(GameObject owner, params IAnimationState[] states)
        {
            _owner = owner;

            foreach (var state in states)
            {
                _states[state.GetType()] = state;
            }
        }

        private readonly Dictionary<Type, IAnimationState> _states = new();
        private IAnimationState _currentState;
        private GameObject _owner;

        public IAnimationState CurrentState => _currentState;

        /// <summary>
        /// Changes to a new state of type T
        /// </summary>
        /// <typeparam name="T">Type of state to change to</typeparam>
        /// <param name="owner">Owner GameObject for debugging</param>
        public void ChangeState<T>(GameObject owner) where T : IAnimationState
        {
            var newStateType = typeof(T);
            if (!_states.TryGetValue(newStateType, out var newState))
                throw new ArgumentException($"State {newStateType.Name} not registered");

            if (_currentState != null && !_currentState.CanTransitionTo(newState))
                return;

            _currentState?.OnExit();
            _currentState = newState;
            _currentState.OnEnter();
        }

        /// <summary>
        /// Updates the current state
        /// </summary>
        public void Update()
        {
            _currentState?.OnUpdate();
        }
    }
}