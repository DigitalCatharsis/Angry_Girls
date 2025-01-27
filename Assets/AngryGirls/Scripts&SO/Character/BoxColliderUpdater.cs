using UnityEngine;

namespace Angry_Girls
{
    public class BoxColliderUpdater : SubComponent
    {
        public bool isUpdatingSpheres = false;
        public bool isUpdateColliderCondition = false;

        public override void OnComponentEnable()
        {
        }

        public override void OnFixedUpdate()
        {
        }

        public void UpdateCollider()
        {
            var stateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            var stateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, stateHash);

            //if (!control.characterSettings.boxcolliderContainer.boxColliderAnimationData.ContainsKey(control.currentStateData.currentStateName))
            if (!control.characterSettings.boxcolliderContainer.boxColliderAnimationData.ContainsKey(stateName))
            {
                return;
            }

            isUpdateColliderCondition = true;

            isUpdatingSpheres = false;

            UpdateBoxCollider_Size();
            UpdateBoxCollider_Center();

            if (isUpdatingSpheres)
            {
                control.subComponentMediator.Notify_UpdatingColliderSpheres(this);
            }
        }

        private void UpdateBoxCollider_Size()
        {
            if (isUpdateColliderCondition == false)
            {
                return;
            }

            var stateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            var stateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, stateHash);

            //var data = control.characterSettings.boxcolliderContainer.boxColliderAnimationData[currentState.currentStateName];
            var data = control.characterSettings.boxcolliderContainer.boxColliderAnimationData[stateName];
            if (Vector3.SqrMagnitude(control.boxCollider.size - data.boxColliderSize) > 0.00001f || Vector3.SqrMagnitude(control.boxCollider.size - data.boxColliderSize) < 0.00001f)
            {
                control.boxCollider.size = Vector3.Lerp(
                    control.boxCollider.size,
                    data.boxColliderSize,
                    Time.deltaTime * data.changeSpeed);

                isUpdatingSpheres = true;
            }
        }

        private void UpdateBoxCollider_Center()
        {
            if (isUpdateColliderCondition == false)
            {
                return;
            }

            var stateHash = control.animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            var stateName = GameLoader.Instance.hashManager.GetName(GameLoader.Instance.statesContainer.stateNames_Dictionary, stateHash);

            var data = control.characterSettings.boxcolliderContainer.boxColliderAnimationData[stateName];
            if (Vector3.SqrMagnitude(control.boxCollider.center - data.boxColliderCenter) > 0.00001f || Vector3.SqrMagnitude(control.boxCollider.center - data.boxColliderCenter) < 0.00001f)
            {
                control.boxCollider.center = Vector3.Lerp(
                    control.boxCollider.center,
                    data.boxColliderCenter,
                    Time.deltaTime * data.changeSpeed);

                isUpdatingSpheres = true;
            }
        }

        public override void OnAwake()
        {
        }
        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
            UpdateCollider();
        }

        public override void OnLateUpdate()
        {
        }
    }
}