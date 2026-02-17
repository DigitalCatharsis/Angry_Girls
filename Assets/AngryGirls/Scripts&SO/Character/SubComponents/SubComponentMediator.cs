using UnityEngine;

namespace Angry_Girls
{
    public class SubComponentMediator : MonoBehaviour
    {
        private CControl _control;
        private AnimationProcessor _animationProcessor;

        public void OnAwake()
        {
            InitComponents();
        }

        private void InitComponents()
        {
            _control = GetComponentInParent<CControl>();
            _animationProcessor = GetComponentInChildren<AnimationProcessor>();
        }


        public void NotifyDeathZoneContact()
        {
            _control.Health.ApplyDamage(_control.Health.CurrentHealth);
            CheckDeath();
            GameplayCoreManager.Instance.CameraManager.StopCameraFollowForRigidBody();
        }
    }
}