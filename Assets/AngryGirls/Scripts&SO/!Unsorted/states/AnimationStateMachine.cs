using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
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


        public void ChangeAnimationState(int newStateHash, float transitionDuration = 0.1f, int layer = 0)
        {
            Debug.Log(_owner + " called ChangeAnimationState for " + GameLoader.Instance.statesContainer.GetStateNameByHash(newStateHash));
            if (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == newStateHash) return;
            _animator.Play(newStateHash, layer, transitionDuration);
            Debug.Log(_owner + " started Animation.Play for " + GameLoader.Instance.statesContainer.GetStateNameByHash(newStateHash));
        }

        public void ChangeAnimationStateFixedTime(int newStateHash, float transitionDuration = 0f, int layer = 0)
        {
            Debug.Log(_owner + " called ChangeAnimationStateFixedTime for " + GameLoader.Instance.statesContainer.GetStateNameByHash(newStateHash));
            if (_animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == newStateHash) return;
            _animator.PlayInFixedTime(newStateHash, layer, transitionDuration);
            Debug.Log(_owner + " started changed animation fixed for " + GameLoader.Instance.statesContainer.GetStateNameByHash(newStateHash));
        }

        public void ChangeAnimationStateCrossFade(int newStateHash, float transitionDuration = 0.1f, int layer = 0)
        {
            Debug.Log(_owner + " called ChangeAnimationStateCrossFade for " + GameLoader.Instance.statesContainer.GetStateNameByHash(newStateHash));
            var currentHash = _animator.GetCurrentAnimatorStateInfo(layer).shortNameHash;

            if (_hashManager.GetName(_statesContainer.stateNames_Dictionary, newStateHash) == StateNames.NONE) return;
            //if (currentHash == newStateHash || _animator.IsInTransition(layer)) return;

            _animator.CrossFadeInFixedTime(
                newStateHash,
                fixedTransitionDuration: transitionDuration,
                layer: layer,
                fixedTimeOffset: 0f,
                normalizedTransitionTime: 0.0f);
            Debug.Log(_owner + " started changeCrossfadefixed for " + GameLoader.Instance.statesContainer.GetStateNameByHash(newStateHash));
        }
    }

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


        public void ChangeState<T>(GameObject owner) where T : IAnimationState
        {
            var newStateType = typeof(T);
            if (!_states.TryGetValue(newStateType, out var newState))
                throw new ArgumentException($"State {newStateType.Name} not registered");

            if (_currentState != null && !_currentState.CanTransitionTo(newState))
                return;

            ColorDebugLog.Log(owner.name + " changing from State: " + _currentState + " to state: " + newState, System.Drawing.KnownColor.Yellow);
            _currentState?.OnExit();
            _currentState = newState;
            _currentState.OnEnter();
        }

        public void Update()
        {
            Debug.Log(_owner.name + " starts to update state: " +_currentState);
            _currentState?.OnUpdate();
        }
    }
}