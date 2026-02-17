using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Finite State Machine for animation states
    /// </summary>
    public class AnimationStateMachine
    {
        public AnimationStateMachine(IAnimationState[] states)
        {

            foreach (var state in states)
            {
                _states[state.GetType()] = state;
            }
        }

        private readonly Dictionary<Type, IAnimationState> _states = new();
        private IAnimationState _currentState;

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