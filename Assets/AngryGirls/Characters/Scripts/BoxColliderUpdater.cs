using UnityEngine;

namespace Angry_Girls
{
    public class BoxColliderUpdater : SubComponent
    {
        [SerializeField] private BoxColliderUpdater_Container _boxColliderUpdater;
        public override void OnAwake()
        {
        }

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.boxColliderUpdater = this;
        }

        public override void OnFixedUpdate()
        {
            UpdateCollider();
        }

        public override void OnLateUpdate()
        {
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
        }
        private void UpdateCollider()
        {
            var currentState = control.subComponentProcessor.animationProcessor.currentStateData;

            if (!_boxColliderUpdater.boxColliderAnimationData.ContainsKey(currentState.currentStateName))
            {
                //ColorDebugLog.Log("No boxColliderAnimationData for state " + currentState.currentStateName, System.Drawing.KnownColor.Red);
                return;
            }

            UpdateBoxCollider_Size(currentState);
            UpdateBoxCollider_Center(currentState);

            control.subComponentProcessor.collisionSpheres.Reposition_FrontSpheres();
            control.subComponentProcessor.collisionSpheres.Reposition_BottomSpheres();
            control.subComponentProcessor.collisionSpheres.Reposition_BackSpheres();
            control.subComponentProcessor.collisionSpheres.Reposition_UpSpheres();

            if (control.subComponentProcessor.animationProcessor.isLanding)  //prevent bug when idle after catching corner of platform
            {
                control.rigidBody.MovePosition(new Vector3(
                    0f,
                    control.subComponentProcessor.groundDetector.landingPosition.y,
                    this.transform.position.z));
            }

        }

        private void UpdateBoxCollider_Size(CurrentStateData currentState)
        {
            var data = _boxColliderUpdater.boxColliderAnimationData[currentState.currentStateName];
            if (Vector3.SqrMagnitude(control.boxCollider.size - data.boxColliderSize) > 0.00001f || Vector3.SqrMagnitude(control.boxCollider.size - data.boxColliderSize) < 0.00001f)
            {
                control.boxCollider.size = Vector3.Lerp(
                    control.boxCollider.size,
                    data.boxColliderSize,
                    Time.deltaTime * data.changeSpeed);
            }
        }
        private void UpdateBoxCollider_Center(CurrentStateData currentState)
        {
            var data = _boxColliderUpdater.boxColliderAnimationData[currentState.currentStateName];
            if (Vector3.SqrMagnitude(control.boxCollider.center - data.boxColliderCenter) > 0.00001f || Vector3.SqrMagnitude(control.boxCollider.center - data.boxColliderCenter) < 0.00001f)
            {
                control.boxCollider.center = Vector3.Lerp(
                    control.boxCollider.center,
                    data.boxColliderCenter,
                    Time.deltaTime * data.changeSpeed);
            }
        }
    }
}