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
            if (!control.characterSettings.boxcolliderContainer.boxColliderAnimationData.ContainsKey(control.currentStateData.currentStateName))
            {
                return;
            }

            isUpdateColliderCondition = true;

            isUpdatingSpheres = false;

            UpdateBoxCollider_Size(control.currentStateData);
            UpdateBoxCollider_Center(control.currentStateData);

            if (isUpdatingSpheres)
            {
                //ColorDebugLog.Log(this.name + " triggers operation " + SubcomponentMediator_EventNames.Reposition_ColliderSpheres, System.Drawing.KnownColor.ControlLightLight);
                control.subComponentMediator.Notify(this, SubcomponentMediator_EventNames.Reposition_ColliderSpheres);

                //if (control.subComponentProcessor.animationProcessor.isLanding)  //prevent bug when idle after catching corner of platform
                //{
                //    control.rigidBody.MovePosition(new Vector3(
                //        0f,
                //        control.subComponentProcessor.groundDetector.landingPosition.y,
                //        this.transform.position.z));
                //}
            }
        }

        private void UpdateBoxCollider_Size(CurrentStateData currentState)
        {
            if (isUpdateColliderCondition == false)
            {
                return;
            }

            var data = control.characterSettings.boxcolliderContainer.boxColliderAnimationData[currentState.currentStateName];
            if (Vector3.SqrMagnitude(control.boxCollider.size - data.boxColliderSize) > 0.00001f || Vector3.SqrMagnitude(control.boxCollider.size - data.boxColliderSize) < 0.00001f)
            {
                control.boxCollider.size = Vector3.Lerp(
                    control.boxCollider.size,
                    data.boxColliderSize,
                    Time.deltaTime * data.changeSpeed);

                isUpdatingSpheres = true;
            }
        }

        private void UpdateBoxCollider_Center(CurrentStateData currentState)
        {
            if (isUpdateColliderCondition == false)
            {
                return;
            }

            var data = control.characterSettings.boxcolliderContainer.boxColliderAnimationData[currentState.currentStateName];
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