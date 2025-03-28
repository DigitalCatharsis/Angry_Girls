using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public class AnimationController
    {
        private readonly Animator _animator;
        private readonly HashManager _hashManager;
        private readonly StatesContainer _statesContainer;

        public AnimationController(Animator animator, HashManager hashManager, StatesContainer statesContainer)
        {
            _animator = animator;
            _hashManager = hashManager;
            _statesContainer = statesContainer;
        }

        public void ChangeAnimationState(int newStateHash, float transitionDuration = 0.1f, int layer = 0)
        {
            if (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == newStateHash) return;
            _animator.Play(newStateHash, layer, transitionDuration);
        }

        public void ChangeAnimationStateFixedTime(int newStateHash, float transitionDuration = 0f, int layer = 0)
        {
            if (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == newStateHash) return;
            _animator.PlayInFixedTime(newStateHash, layer, transitionDuration);
        }

        public void ChangeAnimationStateCrossFade(int newStateHash, float transitionDuration = 0.1f, int layer = 0)
        {
            var currentHash = _animator.GetCurrentAnimatorStateInfo(layer).shortNameHash;

            if (_hashManager.GetName(_statesContainer.stateNames_Dictionary, newStateHash) == StateNames.NONE) return;
            if (currentHash == newStateHash || _animator.IsInTransition(layer)) return;

            _animator.CrossFadeInFixedTime(
                newStateHash,
                fixedTransitionDuration: transitionDuration,
                layer: layer,
                fixedTimeOffset: 0f,
                normalizedTransitionTime: 0.0f);
        }
    }

    public class AnimationStateMachine
    {
        private readonly Dictionary<Type, IAnimationState> _states = new();
        private IAnimationState _currentState;

        public IAnimationState CurrentState => _currentState;

        public AnimationStateMachine(params IAnimationState[] states)
        {
            foreach (var state in states)
            {
                _states[state.GetType()] = state;
            }
        }

        public void ChangeState<T>() where T : IAnimationState
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

        public void Update()
        {
            _currentState?.OnUpdate();
        }
    }
}