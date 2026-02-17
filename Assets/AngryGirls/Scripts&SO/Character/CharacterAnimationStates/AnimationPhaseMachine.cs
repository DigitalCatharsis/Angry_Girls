using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Finite phase Machine for animation phase
    /// </summary>
    public class AnimationPhaseMachine
    {
        public AnimationPhaseMachine(IAnimationPhase[] phases)
        {

            foreach (var phase in phases)
            {
                _phases[phase.GetType()] = phase;
            }
        }

        private readonly Dictionary<Type, IAnimationPhase> _phases = new();
        private IAnimationPhase _currentPhase;

        public IAnimationPhase CurrentPhase => _currentPhase;

        /// <summary>
        /// Changes to a new phase of type T
        /// </summary>
        /// <typeparam name="T">Type of phase to change to</typeparam>
        /// <param name="owner">Owner GameObject for debugging</param>
        public void ChangePhase<T>(GameObject owner) where T : IAnimationPhase
        {
            var newPhaseType = typeof(T);
            if (!_phases.TryGetValue(newPhaseType, out var newPhase))
                throw new ArgumentException($"State {newPhaseType.Name} not registered");

            if (_currentPhase != null && !_currentPhase.CanTransitionTo(newPhase))
                return;

            _currentPhase?.OnExit();
            _currentPhase = newPhase;
            _currentPhase.OnEnter();
        }

        /// <summary>
        /// Updates the current phase
        /// </summary>
        public void Update()
        {
            _currentPhase?.OnUpdate();
        }
    }
}