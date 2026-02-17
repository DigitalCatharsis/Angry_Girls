using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Controller for managing animation playback and transitions
    /// </summary>
    public static class AnimationTransitioner
    {
        static AnimationTransitioner() => _hashManager = CoreManager.Instance.HashManager;
        private static HashManager _hashManager;

        /// <summary>
        /// Changes animation state with Play method
        /// </summary>
        public static void ChangeAnimationState(Animator animator ,int newStateHash, float transitionDuration = 0.1f, int layer = 0)
        {
            if (animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == newStateHash) return;
            animator.Play(newStateHash, layer, transitionDuration);
        }

        /// <summary>
        /// Changes animation state with PlayInFixedTime method
        /// </summary>
        public static void ChangeAnimationStateFixedTime(Animator animator, int newStateHash, float transitionDuration = 0f, int layer = 0)
        {
            if (animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == newStateHash) return;
            animator.PlayInFixedTime(newStateHash, layer, transitionDuration);
        }

        /// <summary>
        /// Changes animation state with cross-fade
        /// </summary>
        public static void ChangeAnimationStateCrossFade(Animator animator, int newStateHash, float transitionDuration = 0.1f, int layer = 0)
        {
            var currentHash = animator.GetCurrentAnimatorStateInfo(layer).shortNameHash;

            // Don't transition to NONE states
            if (_hashManager.GetName(StatesContainer.StateNamesDictionary, newStateHash) == StateNames.NONE) return;

            animator.CrossFadeInFixedTime(
                newStateHash,
                fixedTransitionDuration: transitionDuration,
                layer: layer,
                fixedTimeOffset: 0f,
                normalizedTransitionTime: 0.0f);
        }
    }
}