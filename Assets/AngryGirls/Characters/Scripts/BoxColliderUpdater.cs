using UnityEngine;

namespace Angry_Girls
{
    public class BoxColliderUpdater : SubComponent
    {
        public bool isUpdatingSpheres = false;
        public bool isUpdateColliderCondition = false;

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.boxColliderUpdater = this;
        }

        public override void OnFixedUpdate()
        {
        }

        public void UpdateCollider()
        {
            //ColorDebugLog.Log("Updating", System.Drawing.KnownColor.Green);
            var currentState = control.subComponentProcessor.animationProcessor.currentStateData;

            if (!control.characterSettings.boxcolliderContainer.boxColliderAnimationData.ContainsKey(currentState.currentStateName))
            {
                return;
            }

            isUpdateColliderCondition = true;

            isUpdatingSpheres = false;

            UpdateBoxCollider_Size(currentState);
            UpdateBoxCollider_Center(currentState);

            if (isUpdatingSpheres)
            {
                control.subComponentProcessor.collisionSpheres.Reposition_FrontSpheres();
                control.subComponentProcessor.collisionSpheres.Reposition_BottomSpheres();
                control.subComponentProcessor.collisionSpheres.Reposition_BackSpheres();
                control.subComponentProcessor.collisionSpheres.Reposition_UpSpheres();

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