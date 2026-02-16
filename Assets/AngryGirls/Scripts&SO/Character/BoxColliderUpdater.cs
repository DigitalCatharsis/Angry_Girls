using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Updates character box collider based on animation state.
    /// </summary>
    public class BoxColliderUpdater : SubComponent
    {
        public bool isUpdateColliderCondition = false;

        /// <inheritdoc/>
        public override void OnComponentEnable()
        {
        }

        /// <inheritdoc/>
        public override void OnFixedUpdate()
        {
        }

        /// <summary>
        /// Update collider based on current animation state.
        /// </summary>
        public void UpdateCollider()
        {
            var stateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            var stateName = CoreManager.Instance.HashManager.GetName(GameplayCoreManager.Instance.StatesContainer.stateNames_Dictionary, stateHash);

            if (!control.CharacterSettings.boxcolliderContainer.boxColliderAnimationData.ContainsKey(stateName))
            {
                return;
            }

            isUpdateColliderCondition = true;

            UpdateBoxCollider_Size();
            UpdateBoxCollider_Center();
        }

        private void UpdateBoxCollider_Size()
        {
            if (isUpdateColliderCondition == false)
            {
                return;
            }

            var stateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            var stateName = CoreManager.Instance.HashManager.GetName(GameplayCoreManager.Instance.StatesContainer.stateNames_Dictionary, stateHash);

            var data = control.profile.CharacterSettings.boxcolliderContainer.boxColliderAnimationData[stateName];
            if (Vector3.SqrMagnitude(control.boxCollider.size - data.boxColliderSize) > 0.00001f || Vector3.SqrMagnitude(control.boxCollider.size - data.boxColliderSize) < 0.00001f)
            {
                control.boxCollider.size = Vector3.Lerp(
                    control.boxCollider.size,
                    data.boxColliderSize,
                    Time.deltaTime * data.changeSpeed);
            }
        }

        private void UpdateBoxCollider_Center()
        {
            if (isUpdateColliderCondition == false)
            {
                return;
            }

            var stateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            var stateName = CoreManager.Instance.HashManager.GetName(GameplayCoreManager.Instance.StatesContainer.stateNames_Dictionary, stateHash);

            var data = control.CharacterSettings.boxcolliderContainer.boxColliderAnimationData[stateName];
            if (Vector3.SqrMagnitude(control.boxCollider.center - data.boxColliderCenter) > 0.00001f || Vector3.SqrMagnitude(control.boxCollider.center - data.boxColliderCenter) < 0.00001f)
            {
                control.boxCollider.center = Vector3.Lerp(
                    control.boxCollider.center,
                    data.boxColliderCenter,
                    Time.deltaTime * data.changeSpeed);
            }
        }

        /// <inheritdoc/>
        public override void OnAwake()
        {
        }

        /// <inheritdoc/>
        public override void OnStart()
        {
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            UpdateCollider();
        }

        /// <inheritdoc/>
        public override void OnLateUpdate()
        {
        }
    }
}